using System;

namespace ShopAPI.Modals {

    /// <summary>
    /// 上架表
    /// </summary>
    public class GroundingTableModal : GoodsTableModal {
        /// <summary>
        /// 是否上架
        /// </summary>
        /// <value></value>
        public string isPutaway { get; set; }
    }
}