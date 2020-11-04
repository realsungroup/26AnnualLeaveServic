using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
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
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopAPI.Tasks {
    /// <summary>
    /// 获取需要同步的商品
    /// </summary>
    public class GetNeedSyncGoodsListTask {
        /// <summary>
        /// 物料ID表记录 Modal
        /// </summary>
        public class MaterialRecordModal {
            public string material_name { get; set; }
            public string business_ID { get; set; }
            public string material_ID { get; set; }
            public string is_valid { get; set; }
            public string is_selection { get; set; }
        }
        /// <summary>
        /// 商户设置表 Modal
        /// </summary>
        public class commercialTenantSetModal {
            public string business_ID { get; set; }
            public string shop_ID { get; set; }
            public string? commission_rate { get; set; }
            public long? coupon_remain_count { get; set; }
            public long? coupon_end_day { get; set; }
            public string? superior_brand { get; set; }
            public float? sale_price_up { get; set; }
            public float? sale_price_down { get; set; }
            public long? coupon_amount { get; set; }
            public List<MaterialRecordModal> subdata { get; set; }
        }

        public class MaterialListItemModal : MaterialRecordModal {
            public List<TbkDgOptimusMaterialResponse.MapDataDomain> goodsList { get; set; }
        }

        public class NeedSyncGoodsModal : commercialTenantSetModal {
            public List<MaterialListItemModal> materialList { get; set; }
        }

        /// <summary>
        /// 需要同步的淘宝商品列表
        /// </summary>
        /// <typeparam name="NeedSyncGoodsModal"></typeparam>
        /// <returns></returns>
        public List<NeedSyncGoodsModal> needSyncGoodsList = new List<NeedSyncGoodsModal> ();

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <returns></returns>
        public async Task<List<TbkDgOptimusMaterialResponse.MapDataDomain>> run () {
            var commercialTenantSetRes = await getCommercialTenantSet ();
            List<TbkDgOptimusMaterialResponse.MapDataDomain> list = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
            foreach (var item in commercialTenantSetRes.data) {
                
                
                    

                var ret = getCommercialTenantGoodsList (item);
                list.AddRange (ret);
            }
            return list;
        }

        /// <summary>
        /// 获取商户的所有商品
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> getCommercialTenantGoodsList (commercialTenantSetModal record) {
            WriteLine ("开始获取商户的商品，商户编号：" + record.business_ID);

            // 获取物料 id
            var materialIDRecords = record.subdata;

            if (materialIDRecords == null) {
                return new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
            }

            // 筛选出有效的物料ID表的记录
            var validMaterIDRecords = materialIDRecords.Where (item => item.is_valid == "Y").ToList ().GetRange (0, 1);
            // 筛选出非选品库
            var normalRecords = validMaterIDRecords.Where (item => item.is_selection != "Y").ToList ();
            // 筛选出选品库
            var selectionRecords = validMaterIDRecords.Where (item => item.is_selection == "Y").ToList ();

            var normalGoodsArr = getAllNormalGoodsList (normalRecords);
            var selectionGoodsArr = getSelectionGoodsList (selectionRecords);

            WriteLine ("非选品库商品数量:" + normalGoodsArr.Count);
            WriteLine ("选品库商品数量:" + selectionGoodsArr.Count);

            var goodsArr = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
            goodsArr.AddRange (normalGoodsArr);
            goodsArr.AddRange (selectionGoodsArr);
            WriteLine ("商品总数量:" + goodsArr.Count);

            // 筛选出有效的商品
            WriteLine ("开始筛选有效的商品：");
            var validGoodsList = goodsArr.Where (item => isValidGoods (item, record)).ToList ();

            WriteLine ("有效的商品总数量:" + validGoodsList.Count);
            return validGoodsList;
        }

        /// <summary>
        /// 佣金比例大于等于设置的才有效
        /// </summary>
        /// <param name="taobaoCommissionRate"></param>
        /// <param name="conditionCommissionRate"></param>
        /// <returns></returns>
        public bool isCommissionRateValid (string taobaoCommissionRate, string conditionCommissionRate) {
            if (conditionCommissionRate == null) {
                return true;
            }
            if (taobaoCommissionRate == null) {
                return false;
            }
            var aRate = StrToFloat (taobaoCommissionRate);
            var bRate = StrToFloat (conditionCommissionRate);
            if (aRate >= bRate) {
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
        public bool isCouponRemainCountValid (long taobaoCouponRemainCount, long? conditionCouponRemainCount) {
            if (conditionCouponRemainCount == null) {
                return true;
            }
            if (taobaoCouponRemainCount == null) {
                return false;
            }
            if (taobaoCouponRemainCount >= conditionCouponRemainCount) {
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
        public bool isCouponEndDayValid (string taobaoCouponEndTime, long? conditionCouponEndDay) {
            if (taobaoCouponEndTime == null) {
                return true;
            }
            if (conditionCouponEndDay == null) {
                return true;
            }

            var now = DateTime.Now;
            var couponEndTime = UnixTimeStampToDateTime (Convert.ToDouble (taobaoCouponEndTime));
            DateTime target = couponEndTime;
            var distance = (target - now).TotalDays;
            if (distance >= conditionCouponEndDay) {
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
        public bool isSuperiorBrand (string taobaoSuperiorBrand, string? conditionSuperiorBrand) {
            if (conditionSuperiorBrand == null) {
                return true;
            }

            if (taobaoSuperiorBrand == conditionSuperiorBrand) {
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
        public bool isSalePriceValid (string taobaoSalePrice, float? conditionMinPrice, float? conditionMaxPrice) {
            if (taobaoSalePrice == null) {
                return false;
            }
            var _taobaoSalePrice = float.Parse (taobaoSalePrice);

            if (conditionMinPrice == null && conditionMinPrice == null) {
                return true;
            }
            if (conditionMinPrice == null && conditionMaxPrice != null) {
                return _taobaoSalePrice <= conditionMaxPrice;
            }
            if (conditionMaxPrice == null && conditionMinPrice != null) {
                return _taobaoSalePrice >= conditionMinPrice;
            }
            if (conditionMinPrice != null && conditionMaxPrice != null) {
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
        public bool isCouponAmountValid (long taobaoCouponAmount, long? conditionCouponAmount) {
            if (conditionCouponAmount == null) {
                return true;
            }
            if (taobaoCouponAmount == null) {
                return false;
            }
            if (taobaoCouponAmount >= conditionCouponAmount) {
                return true;
            }
            return false;
        }

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> inValid_佣金比例 = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> inValid_优惠券数量 = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> inValid_优惠券结束时间 = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> inValid_是否品牌精选 = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> inValid_价格 = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> inValid_优惠券金额 = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();

        /// <summary>
        /// 是否是有效的商品
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public bool isValidGoods (TbkDgOptimusMaterialResponse.MapDataDomain goods, commercialTenantSetModal conditionRecord) {
            if (!isCommissionRateValid (goods.CommissionRate, conditionRecord.commission_rate)) {
                inValid_佣金比例.Add (goods);
                return false;
            };
            if (!isCouponRemainCountValid (goods.CouponRemainCount, conditionRecord.coupon_remain_count)) {
                inValid_优惠券数量.Add (goods);
                return false;
            }
            if (!isCouponEndDayValid (goods.CouponEndTime, conditionRecord.coupon_end_day)) {
                inValid_优惠券结束时间.Add (goods);
                return false;
            }
            if (!isSuperiorBrand (goods.SuperiorBrand, conditionRecord.superior_brand)) {
                inValid_是否品牌精选.Add (goods);
                return false;
            }
            if (!isSalePriceValid (goods.ZkFinalPrice, conditionRecord.sale_price_down, conditionRecord.sale_price_up)) {
                inValid_价格.Add (goods);
                return false;
            }
            if (!isCouponAmountValid (goods.CouponAmount, conditionRecord.coupon_amount)) {
                inValid_优惠券金额.Add (goods);
                return false;
            }
            return true;
        }

        public float StrToFloat (object FloatString) {
            float result;
            if (FloatString != null) {
                if (float.TryParse (FloatString.ToString (), out result))
                    return result;
                else {
                    return (float) 0.00;
                }
            } else {
                return (float) 0.00;
            }
        }

        public DateTime UnixTimeStampToDateTime (double unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds (unixTimeStamp).ToLocalTime ();
            return dtDateTime;
        }

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> getAllNormalGoodsList (List<MaterialRecordModal> records) {
            var goodsList = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
            foreach (var record in records) {
                var getGoodsTask = new GetGoodsTask ();
                var materialID = Convert.ToInt64 (record.material_ID);
                getGoodsTask.getOneMaterialGoodsList (materialID);
                var goodsArr = getGoodsTask.getGoodsList ();
                goodsList.AddRange (goodsArr);
            }
            return goodsList;
        }

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> getSelectionGoodsList (List<MaterialRecordModal> records) {
            var goodsList = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
            foreach (var record in records) {
                var materialIDList = record.material_ID.Split (",");
                if (materialIDList.Length != 2) {
                    return goodsList;
                }
                var selectionMaterialID = Convert.ToInt64 (materialIDList[0]);
                var goodsMaterialID = Convert.ToInt64 (materialIDList[1]);

                var getSelectionGoodsTask = new GetSelectionGoodsTask (selectionMaterialID, goodsMaterialID);
                getSelectionGoodsTask.start ();

                goodsList.AddRange (getSelectionGoodsTask.goodsList);
            }
            return goodsList;
        }

        /// <summary>
        /// 获取商户设置表记录
        /// </summary>
        /// <returns></returns>
        public async Task<GetTagbleResponseModal<commercialTenantSetModal>> getCommercialTenantSet () {
            var client = new LzRequest (realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });

            var options = new GetTableOptionsModal { subresid = materialResid };

            return await client.getTable<commercialTenantSetModal> (commercialTenantSetResid, options);
        }
    }
}