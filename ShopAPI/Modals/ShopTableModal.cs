using System;

namespace ShopAPI.Modals {
    /// <summary>
    /// 商铺表 Modal
    /// </summary>
    public class ShopTableModal {
        public string business_ID { get; set; }
        public long? before_day { get; set; }
        public long? coupon_remain_count { get; set; }
    }
}