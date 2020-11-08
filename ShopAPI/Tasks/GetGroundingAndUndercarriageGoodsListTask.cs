using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;
using ShopAPI.Http;
using static ShopAPI.Constant;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ShopAPI.Modals;
using ShopAPI.Tasks;

namespace ShopAPI.Tasks {
    /// <summary>
    /// 获取上下架商品
    /// </summary>
    public class GetGroundingAndUndercarriageGoodsListTask {

        public GetGroundingAndUndercarriageGoodsListTask () {
            this.client = new LzRequest (realsunBaseURL);
        }

        private LzRequest client = null;

        /// <summary>
        /// 上下架商品 Modal
        /// </summary>
        public class GetGroundingAndUntercarriageModal {
            /// <summary>
            /// 条件记录（商铺表记录）
            /// </summary>
            /// <value></value>
            public ShopTableModal conditionRecord { get; set; }

            /// <summary>
            /// 商铺编号
            /// </summary>
            /// <value></value>
            public string shopID { get; set; }

            /// <summary>
            /// 上架商品
            /// </summary>
            /// <value></value>
            public List<GoodsTableModal> groundingGoodsList { get; set; }

            /// <summary>
            /// 下架商品
            /// </summary>
            /// <value></value>
            public List<GoodsTableModal> undercarriageGoodsList { get; set; }

        }

        public List<GetGroundingAndUntercarriageModal> getGroundingAndUntercarriageList = new List<GetGroundingAndUntercarriageModal> ();

        public async Task<List<GetGroundingAndUntercarriageModal>> run () {
            var ret = new List<GetGroundingAndUntercarriageModal> ();
            var ctsRecords = await getCommercialTenantSetRecords ();

            foreach (var ctsRecord in ctsRecords) {
                var item = new GetGroundingAndUntercarriageModal ();
                item.shopID = ctsRecord.shop_ID;

                // 获取条件（商铺表记录）
                var conditionRecord = await getConditionRecordByID (item.shopID);
                if (conditionRecord != null) {
                    item.conditionRecord = conditionRecord;
                    // 根据条件获取上架、下架商品
                    var res = await getGoodsListByCondition (item.conditionRecord);

                    item.groundingGoodsList = res["groundingGoodsList"] as List<GoodsTableModal>;
                    item.undercarriageGoodsList = res["undercarriageGoodsList"] as List<GoodsTableModal>;

                    ret.Add (item);
                }

            }

            return ret;
        }

        /// <summary>
        /// 通过条件获取上架、下架商品列表
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public async Task<Hashtable> getGoodsListByCondition (ShopTableModal conditionRecord) {
            var ret = new Hashtable ();

            var groundingGoodsList = new List<GoodsTableModal> ();
            var undercarriageGoodsList = new List<GoodsTableModal> ();

            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });

            var groundingOptions = new GetTableOptionsModal ();
            var undercarriageOptions = new GetTableOptionsModal ();

            groundingOptions.cmswhere = $"bussiness_ID = '{conditionRecord.business_ID}'";
            undercarriageOptions.cmswhere = $"bussiness_ID = '{conditionRecord.business_ID}'";

            // 是否请求下架商品
            var isRequestUndercarriageGoodsList = true;
            if (conditionRecord.before_day != null) {

                var now = DateTime.Now;
                var actualEndDateTime = now.AddDays (Convert.ToDouble (conditionRecord.before_day));
                var actualEndDateTimeStr = actualEndDateTime.ToString ("yyyy'-'MM'-'dd'T'HH':'mm':'ss");

                WriteLine ("conditionRecord.before_day:" + conditionRecord.before_day);
                WriteLine ("now.ToString:" + now.ToString ("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                WriteLine ("actualEndDateTimeStr:" + actualEndDateTimeStr);

                groundingOptions.cmswhere += $" and coupon_end_time > '{actualEndDateTimeStr}'";
                undercarriageOptions.cmswhere += $" and coupon_end_time <= '{actualEndDateTimeStr}'";

            } else {
                isRequestUndercarriageGoodsList = false;
            }

            // 获取上架商品
            var res = await client.getTable<GoodsTableModal> (goodsResid, groundingOptions);
            groundingGoodsList = res.data;

            // 获取下架商品
            if (isRequestUndercarriageGoodsList) {
                var undercarriageGoodsListRes = await client.getTable<GoodsTableModal> (goodsResid, undercarriageOptions);
                undercarriageGoodsList = undercarriageGoodsListRes.data;
            }

            ret.Add ("groundingGoodsList", groundingGoodsList);
            ret.Add ("undercarriageGoodsList", undercarriageGoodsList);

            return ret;
        }

        /// <summary>
        /// 通过商铺编号获取条件记录（商铺记录）
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public async Task<ShopTableModal> getConditionRecordByID (string shopID) {
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var res = await client.getTable<ShopTableModal> (shopResid);

            if (res.data.Count != 0) {
                return res.data[0];
            }
            return null;
        }

        /// <summary>
        /// 获取商户设置表记录
        /// </summary>
        /// <returns></returns>
        public async Task<List<CommercialTenantSetModal>> getCommercialTenantSetRecords () {
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var res = await client.getTable<CommercialTenantSetModal> (commercialTenantSetResid);
            return res.data;
        }

    }
}