using System;
using System.Buffers;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
using Microsoft.AspNetCore.Routing;
using ShopAPI.Modals;
using ShopAPI.Tasks;

namespace ShopAPI.Tasks
{
    /// <summary>
    /// 获取需要上架的商品（最多 100 条）
    /// </summary>
    public class GetGroundingGoodsListTask
    {
        public GetGroundingGoodsListTask()
        {
            this.client = new LzRequest(realsunBaseURL);
        }

        private LzRequest client = null;

        /// <summary>
        /// 需要上架的商品
        /// </summary>
        /// <typeparam name="GoodsTableModal"></typeparam>
        /// <returns></returns>
        private List<GoodsTableModal> goodsList = new List<GoodsTableModal>();

        /// <summary>
        /// 上架商品 Modal
        /// </summary>
        public class GetGroundingModal
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
            /// 上架商品
            /// </summary>
            /// <value></value>
            public List<GoodsTableModal> goodsList { get; set; }
        }


        /// <summary>
        /// 开始运行
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetGroundingModal>> run(string origin)
        {
            var ret = new List<GetGroundingModal>();
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
                WriteLine("执行上架任务参数 origin 错误。origin 值必须为 taobo 或 jd");
                return null;
            }

            if (ctsRecords.Count == 0)
            {
                WriteLine($"运行上架任务时：{origin} 商户设置表得到的记录为空");
                return null;
            }

            CommercialTenantSetModal ctsRecord = ctsRecords[0];

            var getGrounding = new GetGroundingModal();
            getGrounding.shopID = ctsRecord.shop_ID;

            // 获取条件（商铺表记录）
            var conditionRecord = await getConditionRecordByID(getGrounding.shopID);
            if (conditionRecord != null)
            {
                getGrounding.conditionRecord = conditionRecord;
                // 根据条件获取上架商品
                await getGoodsListByCondition(getGrounding.conditionRecord, origin);
                getGrounding.goodsList = goodsList;
                ret.Add(getGrounding);
            }

            return ret;
        }

        /// <summary>
        /// 通过条件获取上架商品列表
        /// </summary>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public async Task<object> getGoodsListByCondition(ShopTableModal conditionRecord, string origin)
        {
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});

            var options = new GetTableOptionsModal();

            options.cmswhere = getCmswhere(conditionRecord, origin);
            WriteLine($"获取上架商品的 cmswhere:{options.cmswhere}");

            // 只获取第一页的的 100 条数据
            options.pageindex = "0";
            options.pagesize = "100";

            // 获取上架商品
            await getGoodsList(options);
            return new { };
        }

        /// <summary>
        /// 获取 cmswhere
        /// </summary>
        /// <param name="conditionRecord">条件记录</param>
        /// <param name="origin">商品来源</param>
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

                cmswhere += $" and coupon_end_time > '{actualEndDateTimeStr}'";
            }

            // coupon_remain_count 条件（优惠券剩余量）
            if (conditionRecord.coupon_remain_count != null)
            {
                cmswhere += $"coupon_remain_count >= {conditionRecord.coupon_remain_count}";
            }

            return cmswhere;
        }

        /// <summary>
        /// 获取需要上架的商品
        /// </summary>
        /// <returns></returns>
        public async Task<object> getGoodsList(GetTableOptionsModal options)
        {
            var ret = new { };
            try
            {
                var res = await client.getTable<GoodsTableModal>(nullGoodsResid, options);
                goodsList.AddRange(res.data);
            }
            catch (System.Exception e)
            {
                WriteLine($"获取需要上架的商品出错：{e.Message}");
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

            try
            {
                var res = await client.getTable<ShopTableModal>(shopResid, options);
                if (res.data.Count != 0)
                {
                    return res.data[0];
                }

                return null;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取商户设置表记录
        /// </summary>
        /// <returns></returns>
        public async Task<List<CommercialTenantSetModal>> getCommercialTenantSetRecords()
        {
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});
            var res = await client.getTable<CommercialTenantSetModal>(commercialTenantSetResid);
            return res.data;
        }
    }
}