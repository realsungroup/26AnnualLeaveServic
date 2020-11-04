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
using ShopAPI.Jobs;
using static ShopAPI.Constant;
using ShopAPI.Http;

namespace ShopAPI.Controllers {

    /// <summary>
    /// 执行同步商品的任务
    /// </summary>
    [ApiController]
    [Route ("api/v1/[controller]")]
    public class ExecuteSyncGoodsJobController : ControllerBase {
        /// <summary>
        /// 执行同步商品的任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> executeSyncGoodsJob () {
            var res = await SyncGoodsJob.start ();
            return Ok (res);
        }
    }
}