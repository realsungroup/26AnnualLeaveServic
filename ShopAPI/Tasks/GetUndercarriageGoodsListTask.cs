using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;
using ShopAPI.Http;
using static ShopAPI.Constant;
using System.Collections.Generic;
using ShopAPI.Modals;

namespace ShopAPI.Tasks
{
    /// <summary>
    /// 获取下架商品（最多 100 条）
    /// </summary>
    public class GetUndercarriageGoodsListTask
    {
        public GetUndercarriageGoodsListTask()
        {
            this.client = new LzRequest(realsunBaseURL);
        }

        private LzRequest client = null;


        /// <summary>
        /// 需要下架的商品
        /// </summary>
        /// <typeparam name="GoodsTableModal"></typeparam>
        /// <returns></returns>
        public List<GoodsTableModal> goodsList = new List<GoodsTableModal>();

        /// <summary>
        /// 下架商品 Modal
        /// </summary>
        public class GetUntercarriageModal
        {
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
            /// 下架商品
            /// </summary>
            /// <value></value>
            public List<GoodsTableModal> goodsList { get; set; }
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetUntercarriageModal>> run(string origin)
        {
            var ret = new List<GetUntercarriageModal>();
            var ctsRecords = await getCommercialTenantSetRecords();

            CommercialTenantSetModal commercialTenantSetRecord;
            if (origin == "taobao")
            {
                ctsRecords = ctsRecords.Where(x => x.business_ID == taobaoBusinessID).ToList();
            }
            else if (origin == "jd")
            {
                ctsRecords = ctsRecords.Where(x => x.business_ID == jdBusinessID).ToList();
            }
            else
            {
                WriteLine("执行下架任务参数 origin 错误。origin 值必须为 taobo 或 jd");
                return null;
            }

            if (ctsRecords.Count == 0)
            {
                WriteLine($"运行下架任务时：{origin} 商户设置表得到的记录为空");
                return null;
            }

            CommercialTenantSetModal ctsRecord = ctsRecords[0];

            var getUntercarriage = new GetUntercarriageModal();
            getUntercarriage.shopID = ctsRecord.shop_ID;

            // 获取条件（商铺表记录）
            var conditionRecord = await getConditionRecordByID(getUntercarriage.shopID);
            if (conditionRecord != null)
            {
                getUntercarriage.conditionRecord = conditionRecord;
                // 根据条件获取下架商品
                await getGoodsListByCondition(getUntercarriage.conditionRecord, origin);
                getUntercarriage.goodsList = goodsList;

                ret.Add(getUntercarriage);
            }

            return ret;
        }

        /// <summary>
        /// 通过条件获取上架、下架商品列表
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public async Task<Hashtable> getGoodsListByCondition(ShopTableModal conditionRecord, string origin)
        {
            var ret = new Hashtable();
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});

            var options = new GetTableOptionsModal();

            if (hasUndercarriageGoods(conditionRecord))
            {
                options.cmswhere = getCmswhere(conditionRecord, origin);
            }
            else
            {
                return ret;
            }

            options.pageindex = "0";
            options.pagesize = "100";

            await getGoodsList(options);

            return ret;
        }

        /// <summary>
        /// 是否有需要下架的商品
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        private bool hasUndercarriageGoods(ShopTableModal conditionRecord)
        {
            if (conditionRecord.before_day == null && conditionRecord.coupon_remain_count == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取 cmswhere
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public string getCmswhere(ShopTableModal conditionRecord, string origin)
        {
            var cmswhere = $"bussiness_ID = '{conditionRecord.business_ID}' and goods_origin = '{origin}'";

            // before_day 条件
            if (conditionRecord.before_day != null)
            {
                var now = DateTime.Now;
                var actualEndDateTime = now.AddDays(Convert.ToDouble(conditionRecord.before_day));
                var actualEndDateTimeStr = actualEndDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");

                cmswhere += $" and coupon_end_time <= '{actualEndDateTimeStr}'";
            }

            // coupon_remain_count 条件（优惠券剩余量）
            if (conditionRecord.coupon_remain_count != null)
            {
                cmswhere += $"coupon_remain_count < {conditionRecord.coupon_remain_count}";
            }

            return cmswhere;
        }

        /// <summary>
        /// 获取需要下架的商品
        /// </summary>
        /// <returns></returns>
        public async Task<object> getGoodsList(GetTableOptionsModal options)
        {
            var ret = new { };
            try
            {
                var res = await client.getTable<GoodsTableModal>(notUnderGoodsResid, options);
                goodsList.AddRange(res.data);
            }
            catch (System.Exception ex)
            {
                WriteLine($"获取需要下架的商品出错：{ex.Message}");
                return ret;
            }

            return ret;
        }

        /// <summary>
        /// 通过商铺编号获取条件记录（商铺记录）
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public async Task<ShopTableModal> getConditionRecordByID(string shopID)
        {
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});

            var options = new GetTableOptionsModal();
            options.cmswhere = $"shopid = '{shopID}'";
            var res = await client.getTable<ShopTableModal>(shopResid, options);

            if (res.data.Count != 0)
            {
                return res.data[0];
            }

            return null;
        }

        /// <summary>
        /// 获取商户设置表记录
        /// </summary>
        /// <returns></returns>
        public async Task<List<CommercialTenantSetModal>> getCommercialTenantSetRecords()
        {
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});
            try
            {
                var res = await client.getTable<CommercialTenantSetModal>(commercialTenantSetResid);
                return res.data;
            }
            catch (System.Exception ex)
            {
                WriteLine($"获取下架商品时，获取商户设置表记录报错：{ex.Message}");
                return new List<CommercialTenantSetModal>();
            }
        }
    }
}