using System;

namespace ShopAPI.Modals {
    /// <summary>
    /// 物料ID表记录 Modal
    /// </summary>
    public class MaterialRecordModal {
        public string material_name { get; set; }
        public string business_ID { get; set; }
        public string material_ID { get; set; }
        public string is_valid { get; set; }
        public string is_selection { get; set; }
        public int? hot_sell_count { get; set; }
        public string is_hot_sell { get; set; }

    }
}