using System;
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
            /// 符合上面条件的商品
            /// </summary>
            /// <value></value>
            public List<RealsunGoodsModal> goodsList { get; set; }
        }

        public List<GetGroundingAndUntercarriageModal> getGroundingAndUntercarriageList = new List<GetGroundingAndUntercarriageModal> ();

        public async Task<object> run () {
            var ret = new List<GetGroundingAndUntercarriageModal> ();
            var ctsRecords = await getCommercialTenantSetRecords ();

            // 获取所有的商品
            // var allGoodsList = await getAllGoodsList ();

            foreach (var ctsRecord in ctsRecords) {
                var item = new GetGroundingAndUntercarriageModal ();
                item.shopID = ctsRecord.shop_ID;

                // 获取条件（商铺表记录）
                var conditionRecord = await getConditionRecordByID (item.shopID);
                if (conditionRecord != null) {
                    item.conditionRecord = conditionRecord;
                    // 根据条件获取商品
                    item.goodsList = await getGoodsListByCondition (item.conditionRecord);

                }
            }

            return new { };
        }

        /// <summary>
        /// 通过条件获取商品列表
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public async Task<List<RealsunGoodsModal>> getGoodsListByCondition (ShopTableModal conditionRecord) {

            var goodsList = new List<RealsunGoodsModal> ();

            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });

            // var options = new GetTableOptionsModal () {
            //     cmswhere = ""
            // }

            // client.getTable<RealsunGoodsModal> (goodsResid, );
            return goodsList;
        }

        /// <summary>
        /// 获取所有的商品
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public async Task<List<RealsunGoodsModal>> getAllGoodsList () {
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var res = await client.getTable<RealsunGoodsModal> (goodsResid);
            return res.data;
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