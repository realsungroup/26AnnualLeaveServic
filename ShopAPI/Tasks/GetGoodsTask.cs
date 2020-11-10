using System;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using static ShopAPI.Constant;
using System.Collections.Generic;
using static System.Console;
using ShopAPI.Modals;

namespace ShopAPI.Tasks {
    public class GetGoodsTask {

        /// <summary>
        /// 页码
        /// </summary>
        public long pageNo = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public long pageSize = 100;

        public long adzoneId = 110952500231;

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
        /// 获取到的商品
        /// </summary>
        public List<TbkDgOptimusMaterialResponse.MapDataDomain> goodsList = new List<TbkDgOptimusMaterialResponse.MapDataDomain> ();

        /// <summary>
        /// 获取一个物料id下所有的商品
        /// </summary>
        /// <param name="materialId"></param>
        public void getOneMaterialGoodsList (long materialId) {
            WriteLine ("  pageNo:" + pageNo);
            ITopClient client = new DefaultTopClient ("https://eco.taobao.com/router/rest", Constant.appkey, Constant.appsecret, "json");
            TbkDgOptimusMaterialRequest req = new TbkDgOptimusMaterialRequest ();
            req.PageNo = pageNo;
            req.PageSize = pageSize;
            req.AdzoneId = adzoneId;
            req.MaterialId = materialId;
            TbkDgOptimusMaterialResponse rsp = null;
            try {
                rsp = client.Execute (req);
            } catch (System.Exception) { }

            if (rsp == null || rsp.ResultList == null) {
                WriteLine ("  over...");
                return;
            }
            WriteLine ("  rsp.ResultList.Count:" + rsp.ResultList.Count);

            goodsList.AddRange (rsp.ResultList);
            if (hasNextPage (rsp)) {
                pageNo += 1;
                getOneMaterialGoodsList (materialId);
            } else {
                WriteLine ("  over...");
            }
        }

    }
}