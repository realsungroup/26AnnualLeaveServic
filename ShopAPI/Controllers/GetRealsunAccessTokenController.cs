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
    /// 获取 realsun 平台的 accessToken
    /// </summary>
    [ApiController]
    [Route ("api/v1/[controller]")]
    public class GetRealsunAccessTokenController : ControllerBase {
        /// <summary>
        /// 获取 realsun 平台的 accessToken
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string getRealsunAccessToken () {
            return Constant.realsunAccessToken;
        }
    }
}