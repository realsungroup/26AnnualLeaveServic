using System;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopAPI.Modals {
    /// <summary>
    /// 通过淘宝 api 返回的 Modal
    /// </summary>
    public class TaobaoGoodsModal : TbkDgOptimusMaterialResponse.MapDataDomain {
        /// <summary>
        /// 选品库名称（只有选品库的商品才有这个字段）
        /// </summary>
        /// <value></value>
        public string favoritesTitle { get; set; }

        /// <summary>
        /// 物料 id
        /// </summary>
        /// <value></value>
        public string materialID { get; set; }
    }
}