using System;
using System.Runtime.CompilerServices;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using static ShopAPI.Constant;
using System.Collections.Generic;
using static System.Console;
using ShopAPI.Modals;

namespace ShopAPI.Tasks {

    /// <summary>
    /// 获取选品库的商品
    /// </summary>
    public class GetSelectionGoodsTask {

        public GetSelectionGoodsTask (long selectionMaterialId, long goodsMaterialId) {
            this.selectionMaterialId = selectionMaterialId;
            this.goodsMaterialId = goodsMaterialId;
        }

        /// <summary>
        /// 选品库页码
        /// </summary>
        public long selectionPageNo = 1;

        /// <summary>
        /// 选品库每页数量
        /// </summary>
        public long selectionPageSize = 100;

        /// <summary>
        /// 商品页码
        /// </summary>
        public long goodsPageNo = 1;

        /// <summary>
        /// 商品每页数量
        /// </summary>
        public long goodsPageSize = 100;

        public long adzoneId = 110952500231;

        /// <summary>
        /// 获取选品库列表的物料 ID
        /// </summary>
        public long selectionMaterialId = 0;

        /// <summary>
        /// 获取选品库商品的物料 ID
        /// </summary>
        public long goodsMaterialId = 0;

        /// <summary>
        /// 是否还有下一页数据
        /// </summary>
        /// <param name="rsp"></param>
        /// <returns></returns>
        private bool hasNextPage (TbkDgOptimusMaterialResponse rsp) {
            if (rsp.ResultList.Count < 100) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取到的选品库列表
        /// </summary>
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> selectionList = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();

        /// <summary>
        /// 获取到的选品库列表
        /// </summary>
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> goodsList = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();

        public List<FavoritesListItemModal> favoritesList = new List<FavoritesListItemModal> ();

        public void start () {
            getSelectionList ();
            getAllGoodsList ();
        }

        public void getFavoritesList () {
            var list = new List<FavoritesListItemModal> ();
            foreach (var selectionItem in selectionList) {
                if (selectionItem.FavoritesInfo.TotalCount != 0) {
                    foreach (var item in selectionItem.FavoritesInfo.FavoritesList) {
                        list.Add (new FavoritesListItemModal () {
                            favoritesID = item.FavoritesId,
                                favoritesTitle = item.FavoritesTitle
                        });
                    }
                }
            }
            favoritesList = list;
        }

        /// <summary>
        /// 获取选品库列表
        /// </summary>
        public void getSelectionList () {
            ITopClient client = new DefaultTopClient ("https://eco.taobao.com/router/rest", Constant.appkey, Constant.appsecret, "json");
            TbkDgOptimusMaterialRequest req = new TbkDgOptimusMaterialRequest ();
            req.PageNo = selectionPageNo;
            req.PageSize = selectionPageSize;
            req.AdzoneId = adzoneId;
            req.MaterialId = selectionMaterialId;
            TbkDgOptimusMaterialResponse rsp = client.Execute (req);
            if (rsp == null || rsp.ResultList == null) {
                return;
            }

            selectionList.AddRange (rsp.ResultList);
            if (hasNextPage (rsp)) {
                selectionPageNo += 1;
                getSelectionList ();
            } else {
                getFavoritesList ();
            }
        }

        public void getAllGoodsList () {
            foreach (var favoritesListItem in favoritesList) {
                getGoodsList (favoritesListItem);
                favoritesListItem.goodsList = goodsList;
                goodsList = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();
            }
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        public void getGoodsList (FavoritesListItemModal favoritesListItem) {
            var id = favoritesListItem.favoritesID;
            var title = favoritesListItem.favoritesTitle;
            ITopClient client = new DefaultTopClient ("https://eco.taobao.com/router/rest", Constant.appkey, Constant.appsecret, "json");
            TbkDgOptimusMaterialRequest req = new TbkDgOptimusMaterialRequest ();
            req.PageNo = goodsPageNo;
            req.PageSize = goodsPageSize;
            req.AdzoneId = adzoneId;
            req.MaterialId = goodsMaterialId;
            req.FavoritesId = id + "";

            TbkDgOptimusMaterialResponse rsp = client.Execute (req);
            if (rsp == null || rsp.ResultList == null) {
                return;
            }

            goodsList.AddRange (rsp.ResultList);
            if (hasNextPage (rsp)) {
                goodsPageNo += 1;
                getGoodsList (favoritesListItem);
            } else {
                goodsPageNo = 1;
            }
        }

    }
}