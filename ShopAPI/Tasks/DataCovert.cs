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
    /// 数据转换类
    /// </summary>
    public class DataCovert {
        
        /// <summary>
        /// 淘宝商品记录转换为 realsun 平台商品记录
        /// </summary>
        /// <param name="taobaoGoodsList">淘宝商品记录</param>
        /// <param name="materialID">物料id</param>
        /// <param name="favoritesTitle">当商品为选品库商品时，才会有 favoritesTitle</param>
        /// <returns></returns>
        public static List<RealsunGoodsModal> taobaoGoodsList2realsunGoodsList (List<TbkDgOptimusMaterialResponse.MapDataDomain> taobaoGoodsList, string materialID, string favoritesTitle = "") {
            var ret = new List<RealsunGoodsModal> ();
            var i = 1;
            foreach (var item in taobaoGoodsList) {
                var goodsPhotos = "";
                var word = "";

                if (item.SmallImages != null) {
                    foreach (var photoStr in item.SmallImages) {
                        if (goodsPhotos.Length > 0) {
                            goodsPhotos = goodsPhotos + ";" + photoStr;
                        } else {
                            goodsPhotos += photoStr;
                        }
                    }
                }
                if (item.WordList != null) {
                    foreach (var wordListItem in item.WordList) {
                        if (word.Length > 0) {
                            word = word + ";" + word;
                        } else {
                            word += word;
                        }
                    }
                }

                var goodsItem = new RealsunGoodsModal {
                    _id = i++,
                    _state = "editoradd",
                    goods_name = item.Title,
                    goods_img = item.PictUrl,
                    goods_price = float.Parse (item.ZkFinalPrice),
                    goods_dec = item.ItemDescription,
                    goods_photos = goodsPhotos,
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
                    coupon_start_time = item.CouponStartTime,
                    seller_id = item.SellerId,
                    volume = item.Volume,
                    coupon_end_time = item.CouponEndTime,
                    click_url = item.ClickUrl,
                    level_one_category_id = item.LevelOneCategoryId,
                    category_name = item.CategoryName,
                    white_image = item.WhiteImage,
                    word = word,
                    uv_sum_pre_sale = item.UvSumPreSale,
                    coupon_share_url = item.CouponShareUrl,
                    nick = item.Nick,
                    reserve_price = item.ReservePrice,
                    sale_price = item.SalePrice,
                    sub_title = item.SubTitle,
                    superior_brand = Convert.ToInt64 (item.SuperiorBrand),
                    goods_origin = "taobao",
                    material_id = materialID,
                    favorites_title = favoritesTitle,
                    coupon_click_url = item.CouponClickUrl,
                    zk_final_price = item.ZkFinalPrice,
                };
                ret.Add (goodsItem);
            }
            return ret;
        }
    }
}