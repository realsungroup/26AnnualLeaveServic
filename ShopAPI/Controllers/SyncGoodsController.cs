using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShopAPI;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopAPI.Controllers {
    /// <summary>
    /// 执行同步商品任务
    /// </summary>
    [ApiController]
    [Route ("api/v1/[controller]")]
    public class SyncGoodsController : ControllerBase {
        /// <summary>
        /// 执行同步商品任务
        /// </summary>
        /// <param name="body">请求体</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> syncGoods () {
            ITopClient client = new DefaultTopClient ("https://eco.taobao.com/router/rest", Constant.appkey, Constant.appsecret, "json");

            TbkDgOptimusMaterialRequest req = new TbkDgOptimusMaterialRequest ();
            req.PageNo = 1L;
            req.PageSize = 20L;
            req.AdzoneId = 110952500231L;
            req.MaterialId = 28026L;
            TbkDgOptimusMaterialResponse rsp = client.Execute (req);

            return Ok (rsp.Body);
        }
    }
}