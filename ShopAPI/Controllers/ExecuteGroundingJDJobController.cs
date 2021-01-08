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
using ShopAPI.Modals;

namespace ShopAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExecuteGroundingJDJobController : ControllerBase
    {
        /// <summary>
        /// 执行京东商品上架任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> executeGroundingJDJob()
        {
            var res = await GroundingJDJob.start();
            return Ok(res);
        }
    }
}