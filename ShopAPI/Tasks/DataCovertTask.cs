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
using Flurl.Util;
using ShopAPI;
using ShopAPI.Modals;
using ShopAPI.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopAPI.Tasks
{
    /// <summary>
    /// 数据转换类
    /// </summary>
    public class DataCovertTask
    {
        /// <summary>
        /// 淘宝商品记录转换为 realsun 平台商品记录
        /// </summary>
        /// <param name="taobaoGoodsList">淘宝商品记录</param>
        /// <param name="materialID">物料id</param>
        /// <param name="favoritesTitle">当商品为选品库商品时，才会有 favoritesTitle</param>
        /// <returns></returns>
        public static List<GoodsTableModal> taobaoGoodsList2realsunGoodsList(
            List<TbkDgOptimusMaterialResponse.MapDataDomain> taobaoGoodsList, string bussinessID, string materialID,
            string favoritesTitle = "")
        {
            var ret = new List<GoodsTableModal>();
            var i = 1;
            foreach (var item in taobaoGoodsList)
            {
                var goodsPhotos = "";
                var word = "";

                if (item.SmallImages != null)
                {
                    foreach (var photoStr in item.SmallImages)
                    {
                        if (goodsPhotos.Length > 0)
                        {
                            goodsPhotos = $"{goodsPhotos};https:{photoStr}";
                        }
                        else
                        {
                            goodsPhotos += $"https:{photoStr}";
                        }
                    }
                }

                if (item.WordList != null)
                {
                    foreach (var wordListItem in item.WordList)
                    {
                        if (word.Length > 0)
                        {
                            word = word + ";" + word;
                        }
                        else
                        {
                            word += word;
                        }
                    }
                }

                var endTime = "";
                if (item.CouponEndTime != null)
                {
                    var dateTime = Utils.UnixTimeStampToDateTime(Convert.ToDouble(item.CouponEndTime) / 1000);
                    endTime = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                }

                var startTime = "";
                if (item.CouponStartTime != null)
                {
                    var dateTime = Utils.UnixTimeStampToDateTime(Convert.ToDouble(item.CouponStartTime) / 1000);
                    startTime = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                }

                var goodsItem = new GoodsTableModal
                {
                    _id = i++,
                    _state = "editoradd",
                    goods_name = item.Title,
                    goods_img = $"https:{item.PictUrl}",
                    goods_price = float.Parse(item.ZkFinalPrice),
                    goods_dec = item.ItemDescription == null ? item.SubTitle : item.ItemDescription,
                    goods_photos = goodsPhotos,
                    bussiness_ID = bussinessID,
                    coupon_amount = item.CouponAmount,
                    small_images = goodsPhotos,
                    shop_title = item.ShopTitle,
                    category_id = item.CategoryId,
                    coupon_start_fee = item.CouponStartFee,
                    item_id = item.ItemId,
                    coupon_total_count = item.CouponTotalCount,
                    user_type = item.UserType,
                    coupon_remain_count = item.CouponRemainCount,
                    commission_rate = item.CommissionRate,
                    coupon_start_time = startTime,
                    seller_id = item.SellerId,
                    volume = item.Volume,
                    coupon_end_time = endTime,
                    click_url = $"https:{item.ClickUrl}",
                    level_one_category_id = item.LevelOneCategoryId,
                    category_name = item.CategoryName,
                    white_image = item.WhiteImage,
                    word = word,
                    uv_sum_pre_sale = item.UvSumPreSale,
                    coupon_share_url = $"https:{item.CouponShareUrl}",
                    nick = item.Nick,
                    reserve_price = item.ReservePrice,
                    sale_price = item.SalePrice,
                    sub_title = item.SubTitle,
                    superior_brand = Convert.ToInt64(item.SuperiorBrand),
                    goods_origin = "taobao",
                    material_id = materialID,
                    favorites_title = favoritesTitle,
                    coupon_click_url = $"https:{item.CouponClickUrl}",
                    zk_final_price = item.ZkFinalPrice,
                };
                ret.Add(goodsItem);
            }

            return ret;
        }

        /// <summary>
        /// 商品表记录转为上架表记录
        /// </summary>
        /// <param name="shopTableList">商品记录</param>
        /// <param name="isPutaway">是否上架</param>
        /// <param name="shopID">商铺ID</param>
        /// <returns></returns>
        public static List<GroundingTableModal> goodsTalbe2GroundingTable(List<GoodsTableModal> shopTableList,
            string isPutaway, string shopID)
        {
            var ret = new List<GroundingTableModal>();

            var i = 1;

            foreach (var item in shopTableList)
            {
                var newItem = new GroundingTableModal
                {
                    _id = i++,
                    _state = "editoradd",
                    isPutaway = isPutaway,
                    goods_id = item.goods_id,
                    shop_ID = shopID,

                    goods_name = item.goods_name,
                    goods_img = item.goods_img,
                    goods_price = item.goods_price,
                    goods_dec = item.goods_dec,
                    goods_photos = item.goods_photos,
                    bussiness_ID = item.bussiness_ID,
                    coupon_amount = item.coupon_amount,
                    small_images = item.small_images,
                    shop_title = item.shop_title,
                    category_id = item.category_id,
                    coupon_start_fee = item.coupon_start_fee,
                    item_id = item.item_id,
                    coupon_total_count = item.coupon_total_count,
                    user_type = item.user_type,
                    coupon_remain_count = item.coupon_remain_count,
                    commission_rate = item.commission_rate,
                    coupon_start_time = item.coupon_start_time,
                    seller_id = item.seller_id,
                    volume = item.volume,
                    coupon_end_time = item.coupon_end_time,
                    click_url = item.click_url,
                    level_one_category_id = item.level_one_category_id,
                    category_name = item.category_name,
                    white_image = item.white_image,
                    word = item.word,
                    uv_sum_pre_sale = item.uv_sum_pre_sale,
                    coupon_share_url = item.coupon_share_url,
                    nick = item.nick,
                    reserve_price = item.reserve_price,
                    sale_price = item.sale_price,
                    sub_title = item.sub_title,
                    superior_brand = item.superior_brand,
                    goods_origin = item.goods_origin,
                    material_id = item.material_id,
                    favorites_title = item.favorites_title,
                    coupon_click_url = item.coupon_click_url,
                    zk_final_price = item.zk_final_price,
                };

                ret.Add(newItem);
            }

            return ret;
        }

        /// <summary>
        /// 京东商品单条记录转换为 realsun 平台商品记录
        /// </summary>
        /// <param name="taobaoGoodsList">淘宝商品记录</param>
        /// <param name="materialID">物料id</param>
        /// <param name="favoritesTitle">当商品为选品库商品时，才会有 favoritesTitle</param>
        /// <returns></returns>
        public static GoodsTableModal JDGoods2realsunGoods(QueryResult.JFGoodsResp goods, string bussinessID)
        {
            var item = goods;

            string white_image = "";
            string goods_photos = "";
            if (item.imageInfo != null)
            {
                white_image = item.imageInfo.whiteImage;

                if (item.imageInfo.imageList.Count != 0)
                {
                    foreach (var urlInfo in item.imageInfo.imageList)
                    {
                        if (urlInfo != null)
                        {
                            if (goods_photos.Length > 0)
                            {
                                goods_photos = $"{goods_photos};{urlInfo.url}";
                            }
                            else
                            {
                                goods_photos = $"{urlInfo.url}";
                            }
                        }
                    }
                }
            }

            float goods_price = 0;
            if (item.priceInfo != null)
            {
                goods_price = (float) item.priceInfo.price;
            }

            string goods_dec = "";
            if (item.documentInfo != null)
            {
                goods_dec = item.documentInfo.document;
            }

            string commission_rate = "";
            if (item.commissionInfo != null)
            {
                commission_rate = item.commissionInfo.commissionShare.ToString("F2");
            }

            string coupon_end_time = "";
            long coupon_amount = 0;
            if (item.couponInfo != null)
            {
                if (item.couponInfo.couponList.Count != 0)
                {
                    coupon_end_time = item.couponInfo.couponList[0].getEndTime.ToString();
                    coupon_amount = (long) item.couponInfo.couponList[0].discount;
                }
            }

            var goodsItem = new GoodsTableModal
            {
                _state = "editoradd",
                item_id = item.skuId,
                goods_name = item.skuName,
                white_image = white_image,
                goods_photos = goods_photos,
                goods_img = white_image,
                goods_price = goods_price,
                goods_dec = goods_dec,
                comments = item.comments,
                commission_rate = commission_rate,
                goodCommentsShare = item.goodCommentsShare,
                deliveryType = item.deliveryType,
                coupon_end_time = coupon_end_time,
                coupon_amount = coupon_amount,
                coupon_click_url = item.clickURL,
                goods_category = item.categoryInfo.cid1.ToString(),
                goods_categoryname1 = item.categoryInfo.cid1Name,
                goods_category2 = item.categoryInfo.cid2.ToString(),
                goods_categoryname2 = item.categoryInfo.cid2Name,
                goods_category3 = item.categoryInfo.cid3.ToString(),
                goods_categoryname3 = item.categoryInfo.cid3Name,
                material_id = item.resourceInfo.eliteId.ToString(),
                goods_origin = "jd",
                bussiness_ID = bussinessID
            };
            return goodsItem;
        }
    }
}