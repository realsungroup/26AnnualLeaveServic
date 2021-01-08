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
using Microsoft.AspNetCore.Routing;
using ShopAPI.Modals;

namespace ShopAPI.Tasks
{
    /// <summary>
    /// 同步京东商品到 realsun 平台
    /// </summary>
    public class SyncJDGoodsListTask
    {
        // 京东商户id
        public string businessID = "659529153820";

        public SyncJDGoodsListTask()
        {
        }

        public SyncJDGoodsListTask(string materialID, bool debug = false)
        {
            this.materialID = materialID;
            this.debug = debug;
            //  run();
        }

        /// <summary>
        /// 请求的商品的物料ID，用于测试。如果为 null，则请求所有的物料ID的商品；如果有值，则只请求该物料ID的商品
        /// </summary>
        public string materialID = null;

        /// <summary>
        /// 是否是 debug 模式。如果是 debug 模式，则会记录未通过校验的淘宝商品，从实例对象上可以获取未通过校验的商品
        /// </summary>
        public bool debug = false;

        public class MaterialListItemModal : MaterialRecordModal
        {
            public List<object> goodsList { get; set; }
        }

        public class NeedSyncJDGoodsModal
        {
            public List<MaterialListItemModal> materialList { get; set; }

            // 条件
            public CommercialTenantSetModal conditionRecord { get; set; }
        }

        /// <summary>
        /// 需要同步的淘宝商品列表
        /// </summary>
        /// <typeparam name="NeedSyncJDGoodsModal"></typeparam>
        /// <returns></returns>
        public List<NeedSyncJDGoodsModal> needSyncGoodsList = new List<NeedSyncJDGoodsModal>();

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <returns></returns>
        // public async Task<List<GoodsTableModal>> run()
        public async Task<object> run()
        {
            var commercialTenantSetRes = await getCommercialTenantSet();
            // 筛选出京东商户
            var listData =
                commercialTenantSetRes.data.Where(record => record.business_ID == businessID).ToList();
            List<NeedSyncJDGoodsModal> list = new List<NeedSyncJDGoodsModal>();

            await getCommercialTenantGoodsList(listData[0]);
            return null;
        }


        /// <summary>
        /// 同步京东商品
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<object> getCommercialTenantGoodsList(CommercialTenantSetModal record)
        {
            WriteLine("=============================================同步京东商品");
            WriteLine("开始获取商户的商品，商户编号：" + record.business_ID);

            // 获取物料ID
            var materialIDRecords = record.subdata;

            // 筛选物料ID
            if (materialID != null)
            {
                materialIDRecords = materialIDRecords.Where(x => x.material_ID == materialID).ToList();
            }

            if (materialIDRecords == null)
            {
                return null;
            }

            foreach (var materialIDRecord in materialIDRecords)
            {
                // 有效的物料ID
                if (materialIDRecord.is_valid == "Y")
                {
                    WriteLine(" 1.物料ID：" + materialIDRecord.material_ID);
                    await getAndSyncGoodsList(materialIDRecord, record);
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// 佣金比例大于等于设置的才有效
        /// </summary>
        /// <param name="taobaoCommissionRate"></param>
        /// <param name="conditionCommissionRate"></param>
        /// <returns></returns>
        public bool isCommissionRateValid(string taobaoCommissionRate, string conditionCommissionRate)
        {
            if (conditionCommissionRate == null)
            {
                return true;
            }

            if (taobaoCommissionRate == null)
            {
                return false;
            }

            var aRate = StrToFloat(taobaoCommissionRate);
            var bRate = StrToFloat(conditionCommissionRate);
            if (aRate >= bRate)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 优惠券数量大于等于设置的才有效
        /// </summary>
        /// <param name="taobaoCouponRemainCount"></param>
        /// <param name="conditionCouponRemainCount"></param>
        /// <returns></returns>
        public bool isCouponRemainCountValid(long taobaoCouponRemainCount, long? conditionCouponRemainCount)
        {
            if (conditionCouponRemainCount == null)
            {
                return true;
            }

            if (taobaoCouponRemainCount == null)
            {
                return false;
            }

            if (taobaoCouponRemainCount >= conditionCouponRemainCount)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 优惠券结束时间距离现在的天数大于设置的才有效
        /// </summary>
        /// <param name="taobaoCouponEndTime"></param>
        /// <param name="conditionCouponEndDay"></param>
        /// <returns></returns>
        public bool isCouponEndDayValid(string taobaoCouponEndTime, long? conditionCouponEndDay)
        {
            if (taobaoCouponEndTime == null)
            {
                return true;
            }

            if (conditionCouponEndDay == null)
            {
                return true;
            }

            var now = DateTime.Now;
            var couponEndTime = UnixTimeStampToDateTime(Convert.ToDouble(taobaoCouponEndTime) / 1000);
            DateTime target = couponEndTime;
            var distance = (target - now).TotalDays;
            if (distance >= conditionCouponEndDay)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否品牌精选值要等于设置的才有效
        /// </summary>
        /// <param name="taobaoSuperiorBrand"></param>
        /// <param name="conditionSuperiorBrand"></param>
        /// <returns></returns>
        public bool isSuperiorBrand(string taobaoSuperiorBrand, string? conditionSuperiorBrand)
        {
            if (conditionSuperiorBrand == null)
            {
                return true;
            }

            if (taobaoSuperiorBrand == conditionSuperiorBrand)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 价格验证
        /// </summary>
        /// <param name="taobaoSalePrice">淘宝商品价格</param>
        /// <param name="conditionMinPrice">最小价格</param>
        /// <param name="conditionMaxPrice">最大价格</param>
        /// <returns></returns>
        public bool isSalePriceValid(string taobaoSalePrice, float? conditionMinPrice, float? conditionMaxPrice)
        {
            if (taobaoSalePrice == null)
            {
                return false;
            }

            var _taobaoSalePrice = float.Parse(taobaoSalePrice);

            if (conditionMinPrice == null && conditionMinPrice == null)
            {
                return true;
            }

            if (conditionMinPrice == null && conditionMaxPrice != null)
            {
                return _taobaoSalePrice <= conditionMaxPrice;
            }

            if (conditionMaxPrice == null && conditionMinPrice != null)
            {
                return _taobaoSalePrice >= conditionMinPrice;
            }

            if (conditionMinPrice != null && conditionMaxPrice != null)
            {
                return _taobaoSalePrice >= conditionMinPrice && _taobaoSalePrice <= conditionMaxPrice;
            }

            return false;
        }

        /// <summary>
        /// 优惠券金额大于等于设置的才有效
        /// </summary>
        /// <param name="taobaoCouponAmount"></param>
        /// <param name="conditionCouponAmount"></param>
        /// <returns></returns>
        public bool isCouponAmountValid(long taobaoCouponAmount, long? conditionCouponAmount)
        {
            if (conditionCouponAmount == null)
            {
                return true;
            }

            if (taobaoCouponAmount == null)
            {
                return false;
            }

            if (taobaoCouponAmount >= conditionCouponAmount)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否是有效的商品
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public bool isValidGoods(GoodsTableModal goods,
            CommercialTenantSetModal conditionRecord)
        {
            if (!isCommissionRateValid(goods.commission_rate, conditionRecord.commission_rate))
            {
                return false;
            }

            // if (!isCouponEndDayValid(goods.CouponEndTime, conditionRecord.coupon_end_day))
            // {
            //     return false;
            // }

            if (!isSalePriceValid(goods.goods_price.ToString(), conditionRecord.sale_price_down,
                conditionRecord.sale_price_up))
            {
                return false;
            }

            return true;
        }

        public float StrToFloat(object FloatString)
        {
            float result;
            if (FloatString != null)
            {
                if (float.TryParse(FloatString.ToString(), out result))
                    return result;
                else
                {
                    return (float) 0.00;
                }
            }
            else
            {
                return (float) 0.00;
            }
        }

        public DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }


        public async Task<List<GoodsTableModal>> getOnePageGoodsList(long materialID,
            CommercialTenantSetModal conditionRecord, int pageIndex, int pageSize)
        {
            var validGoodsList = new List<GoodsTableModal>();

            // 获取到商品
            var queryResult = await JDHttp.GetJDGoodsList(Convert.ToInt32(materialID), pageIndex, pageSize);

            var result = Math.Ceiling(Convert.ToDouble(queryResult.totalCount) / Convert.ToDouble(pageSize));
            totalPages = (int) result;

            // 获取推广链接
            if (queryResult != null && queryResult.data.Count != 0)
            {
                var index = 1;
                foreach (var goodsItem in queryResult.data)
                {
                    var realsunGoods = DataCovertTask.JDGoods2realsunGoods(goodsItem, conditionRecord.business_ID);
                    realsunGoods._id = index++;
                    if (goodsItem.materialUrl != null && isValidGoods(realsunGoods, conditionRecord))
                    {
                        var getResult = await JDHttp.GetPromotionLink(goodsItem.materialUrl);

                        if (getResult != null && getResult.data != null && getResult.data.clickURL != null)
                        {
                            realsunGoods.coupon_click_url = getResult.data.clickURL;
                        }

                        validGoodsList.Add(realsunGoods);
                    }
                }
            }

            // 转换成商品表的记录
            return validGoodsList;
        }


        private int pageIndex = 1;
        private int pageSize = 20;
        private int totalPages = 0;

        /// <summary>
        /// 获取京东的商品并且同步
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<object> getAndSyncGoodsList(MaterialRecordModal record,
            CommercialTenantSetModal conditionRecord)
        {
            var materialID = Convert.ToInt64(record.material_ID);

            WriteLine("pageIndex:" + pageIndex);

            // 获取到了能够同步到商品表的商品记录
            var goodsList = await getOnePageGoodsList(materialID, conditionRecord, pageIndex, pageSize);

            pageIndex++;
            // 将商品添加到 realsun 平台的商品表
            WriteLine("同步的商品数量：" + goodsList.Count);
            await addGoodsToRealsun(goodsList);

            if (pageIndex < totalPages)
            {
                return await getAndSyncGoodsList(record, conditionRecord);
            }

            return new { };
        }

        /// <summary>
        /// 添加商品到 realsun 平台
        /// </summary>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        public static async Task<object> addGoodsToRealsun(List<GoodsTableModal> goodsList)
        {
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});
            try
            {
                // 同步商品
                WriteLine($"开始同步京东商品，数量：{goodsList.Count}");
                await client.AddRecords<object>(goodsResid, goodsList);
            }
            catch (System.Exception e)
            {
                WriteLine("同步商品出错：" + e.Message);
            }

            return new { };
        }


        /// <summary>
        /// 获取商户设置表记录
        /// </summary>
        /// <returns></returns>
        public async Task<GetTagbleResponseModal<CommercialTenantSetModal>> getCommercialTenantSet()
        {
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});

            var options = new GetTableOptionsModal {subresid = materialResid};

            return await client.getTable<CommercialTenantSetModal>(commercialTenantSetResid, options);
        }
    }
}