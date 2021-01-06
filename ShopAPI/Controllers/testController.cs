using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShopAPI.Jobs;
using System.Web;
using ShopAPI.Http;
using static ShopAPI.Constant;
using ShopAPI.Modals;
using ShopAPI.Http;

namespace ShopAPI.Controllers
{
    /// <summary>
    /// 获取 realsun 平台的 accessToken
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// 获取 realsun 平台的 accessToken
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> test()
        {
            SyncJDGoodsJob.start();
          
            return Ok(new
            {
            });
        }
    }
}