using System;

namespace ShopAPI.Modals
{
    /// <summary>
    ///  商品表 Modal
    /// </summary>
    public class GoodsTableModal
    {
        public string goods_id { get; set; }
        public string goods_name { get; set; }
        public string goods_img { get; set; }
        public float? goods_price { get; set; }
        public string goods_dec { get; set; }
        public string goods_photos { get; set; }
        public string bussiness_ID { get; set; }

        // 其他
        public long? coupon_amount { get; set; }
        public string small_images { get; set; }
        public string shop_title { get; set; }
        public long? category_id { get; set; }
        public string coupon_start_fee { get; set; }
        public long? item_id { get; set; }
        public long? coupon_total_count { get; set; }
        public long? user_type { get; set; }
        public long? coupon_remain_count { get; set; }
        public string commission_rate { get; set; }
        public string coupon_start_time { get; set; }
        public long? seller_id { get; set; }
        public long? volume { get; set; }
        public string coupon_end_time { get; set; }
        public string click_url { get; set; }
        public string level_one_category_name { get; set; }
        public long? level_one_category_id { get; set; }
        public string category_name { get; set; }
        public string white_image { get; set; }
        public string word { get; set; }
        public long? uv_sum_pre_sale { get; set; }
        public string coupon_share_url { get; set; }
        public string nick { get; set; }
        public string reserve_price { get; set; }
        public string sale_price { get; set; }
        public string sub_title { get; set; }
        public long? superior_brand { get; set; }
        public string goods_origin { get; set; }
        public string material_id { get; set; }
        public string favorites_title { get; set; }
        public string coupon_click_url { get; set; }
        public string zk_final_price { get; set; }


        /// <summary>
        /// 京东商品特有的字段
        /// </summary>
        public long comments { get; set; }

        public double goodCommentsShare { get; set; }
        public int deliveryType { get; set; }

        public string goods_category { get; set; }
        public string goods_categoryname1 { get; set; }
        public string goods_category2 { get; set; }
        public string goods_categoryname2 { get; set; }
        public string goods_category3 { get; set; }
        public string goods_categoryname3 { get; set; }


        public long _id { get; set; }
        public string _state { get; set; }
    }
}