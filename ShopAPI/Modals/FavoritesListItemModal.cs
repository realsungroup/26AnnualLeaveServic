using System;
using System.Collections.Generic;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopAPI.Modals {
    public class FavoritesListItemModal {
        public long favoritesID { get; set; }
        public string favoritesTitle { get; set; }
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> goodsList { get; set; }
    }

}