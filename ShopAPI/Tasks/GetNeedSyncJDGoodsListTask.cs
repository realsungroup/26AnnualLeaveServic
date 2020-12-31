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
using Top.Api.Response;

namespace ShopAPI.Tasks
{
    /// <summary>
    /// 获取需要同步的商品
    /// </summary>
    public class GetNeedSyncJDGoodsListTask
    {
        // 京东商户id
        public string businessID = "659529153820";

        public GetNeedSyncJDGoodsListTask()
        {
        }

        public GetNeedSyncJDGoodsListTask(string materialID, bool debug = false)
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
            // 是否是选品库商品（如果是，则取 favoritesList，否则直接取 goodsList）
            public bool isSelection { get; set; }

            // isSelection 为 false 时，goodsList 才有值
            public List<TbkDgOptimusMaterialResponse.MapDataDomain> goodsList { get; set; }

            // isSelection 为 true 时，favoritesList 才有值
            public List<FavoritesListItemModal> favoritesList { get; set; }
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
        public async Task<List<GoodsTableModal>> run()
        {
            var commercialTenantSetRes = await getCommercialTenantSet();

            // 筛选出京东商户
            var listData =
                commercialTenantSetRes.data.Where(record => record.business_ID == businessID).ToList();
            List<NeedSyncJDGoodsModal> list = new List<NeedSyncJDGoodsModal>();
            foreach (var item in listData)
            {
                var ret = getCommercialTenantGoodsList(item);
                list.Add(ret);
            }

            WriteLine("list" + list);
            var records = new List<GoodsTableModal>();
            foreach (var item in list)
            {
                var conditionRecord = item.conditionRecord;
                var bussinessID = conditionRecord.business_ID;
                foreach (var materialItem in item.materialList)
                {
                    var validGoods = materialItem.goodsList.Where(x => isValidGoods(x, conditionRecord)).ToList();
                    var newRecords =
                        DataCovertTask.taobaoGoodsList2realsunGoodsList(validGoods, bussinessID,
                            materialItem.material_ID);
                    records.AddRange(newRecords);
                }
            }

            // WriteLine (" 3.需要同步的商品数量：" + records);

            return records;
        }

        /// <summary>
        /// 获取商户的所有商品
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public NeedSyncJDGoodsModal getCommercialTenantGoodsList(CommercialTenantSetModal record)
        {
            WriteLine("=============================================");
            WriteLine("开始获取商户的商品，商户编号：" + record.business_ID);

            var NeedSyncJDGoodsModal = new NeedSyncJDGoodsModal();
            NeedSyncJDGoodsModal.materialList = new List<MaterialListItemModal>();
            NeedSyncJDGoodsModal.conditionRecord = record;

            // 获取物料ID
            var materialIDRecords = record.subdata;

            // 筛选物料ID
            if (materialID != null)
            {
                materialIDRecords = materialIDRecords.Where(x => x.material_ID == materialID).ToList();
            }

            if (materialIDRecords == null)
            {
                return NeedSyncJDGoodsModal;
            }

            foreach (var materialIDRecord in materialIDRecords)
            {
                // 有效的物料ID
                if (materialIDRecord.is_valid == "Y")
                {
                    WriteLine(" 1.物料ID：" + materialIDRecord.material_ID);

                    var materialItem = new MaterialListItemModal();
                    materialItem.material_ID = materialIDRecord.material_ID;
                    //  非选品库
                    if (materialIDRecord.is_selection != "Y")
                    {
                        materialItem.isSelection = false;

                        // 实时热销商品只取前 n 个
                        object count = null;
                        if (materialIDRecord.is_hot_sell == "Y")
                        {
                            if (materialIDRecord.hot_sell_count != null)
                            {
                                count = materialIDRecord.hot_sell_count;
                            }
                        }

                        var goodsList = getNormalGoodsList(materialIDRecord);

                        if (materialIDRecord.is_hot_sell == "Y")
                        {
                            if (count != null)
                            {
                                if (goodsList.Count >= (int) count)
                                {
                                    materialItem.goodsList = goodsList.GetRange(0, (int) count);
                                }
                                else
                                {
                                    materialItem.goodsList = goodsList;
                                }
                            }
                            else
                            {
                                // count 没有的话，则不同步实时热销榜的数据
                                materialItem.goodsList = new List<TbkDgOptimusMaterialResponse.MapDataDomain>();
                            }
                        }
                        else
                        {
                            materialItem.goodsList = goodsList;
                        }

                        WriteLine(" 2.商品数量：" + materialItem.goodsList.Count);
                    }

                    NeedSyncJDGoodsModal.materialList.Add(materialItem);
                }
            }

            return NeedSyncJDGoodsModal;
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

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> _1_inValid_佣金比例 =
            new List<TbkDgOptimusMaterialResponse.MapDataDomain>();

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> _2_inValid_优惠券数量 =
            new List<TbkDgOptimusMaterialResponse.MapDataDomain>();

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> _3_inValid_优惠券结束时间 =
            new List<TbkDgOptimusMaterialResponse.MapDataDomain>();

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> _4_inValid_是否品牌精选 =
            new List<TbkDgOptimusMaterialResponse.MapDataDomain>();

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> _5_inValid_价格 =
            new List<TbkDgOptimusMaterialResponse.MapDataDomain>();

        public List<TbkDgOptimusMaterialResponse.MapDataDomain> _6_inValid_优惠券金额 =
            new List<TbkDgOptimusMaterialResponse.MapDataDomain>();

        /// <summary>
        /// 是否是有效的商品
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="conditionRecord"></param>
        /// <returns></returns>
        public bool isValidGoods(TbkDgOptimusMaterialResponse.MapDataDomain goods,
            CommercialTenantSetModal conditionRecord)
        {
            if (!isCommissionRateValid(goods.CommissionRate, conditionRecord.commission_rate))
            {
                if (debug)
                {
                    _1_inValid_佣金比例.Add(goods);
                }

                return false;
            }

            ;
            if (!isCouponRemainCountValid(goods.CouponRemainCount, conditionRecord.coupon_remain_count))
            {
                if (debug)
                {
                    _2_inValid_优惠券数量.Add(goods);
                }

                return false;
            }

            if (!isCouponEndDayValid(goods.CouponEndTime, conditionRecord.coupon_end_day))
            {
                if (debug)
                {
                    _3_inValid_优惠券结束时间.Add(goods);
                }

                return false;
            }

            if (!isSuperiorBrand(goods.SuperiorBrand, conditionRecord.superior_brand))
            {
                if (debug)
                {
                    _4_inValid_是否品牌精选.Add(goods);
                }

                return false;
            }

            if (!isSalePriceValid(goods.ZkFinalPrice, conditionRecord.sale_price_down, conditionRecord.sale_price_up))
            {
                if (debug)
                {
                    _5_inValid_价格.Add(goods);
                }

                return false;
            }

            if (!isCouponAmountValid(goods.CouponAmount, conditionRecord.coupon_amount))
            {
                if (debug)
                {
                    _6_inValid_优惠券金额.Add(goods);
                }

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

        /// <summary>
        /// 获取非选品物料的商品
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> getNormalGoodsList(MaterialRecordModal record)
        {
            var getGoodsTask = new GetGoodsTask();

            var materialID = Convert.ToInt64(record.material_ID);

            getGoodsTask.getOneMaterialGoodsList(materialID);

            return getGoodsTask.goodsList;
        }

        /// <summary>
        /// 获取选品物料的商品
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public List<FavoritesListItemModal> getSelectionGoodsList(MaterialRecordModal record)
        {
            var goodsList = new List<FavoritesListItemModal>();

            var materialIDList = record.material_ID.Split(",");

            if (materialIDList.Length != 2)
            {
                return goodsList;
            }

            var selectionMaterialID = Convert.ToInt64(materialIDList[0]);
            var goodsMaterialID = Convert.ToInt64(materialIDList[1]);

            var getSelectionGoodsTask = new GetSelectionGoodsTask(selectionMaterialID, goodsMaterialID);
            getSelectionGoodsTask.start();

            return getSelectionGoodsTask.favoritesList;
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