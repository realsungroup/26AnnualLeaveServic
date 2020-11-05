using System.Collections.Generic;
using ShopAPI.Modals;

namespace ShopAPI.Modals {
    /// <summary>
    /// 商户设置表 Modal
    /// </summary>
    public class CommercialTenantSetModal {
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

}