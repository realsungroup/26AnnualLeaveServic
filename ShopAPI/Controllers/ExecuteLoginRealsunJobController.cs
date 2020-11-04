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

namespace ShopAPI.Controllers {
    /// <summary>
    /// 执行登录 realsun 平台任务
    /// </summary>
    [ApiController]
    [Route ("api/v1/[controller]")]
    public class ExecuteLoginRealsunJobController : ControllerBase {
        /// <summary>
        /// 执行登录 realsun 平台任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> executeLoginRealsunJob () {
            var res = await LoginRealsunJob.start ();
            return Ok (res);
        }
    }
}