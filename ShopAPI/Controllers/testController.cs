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
            var now = DateTime.Now;

            var method = "jd.union.open.goods.jingfen.query";
            var accessToken = "";
            var v = "1.0";

            var buyParamJson360 = "{\"goodsReq\":{\"eliteId\":22}}";


            var client = new JDClient(now, jdAppKey, jdAppSecret, method, accessToken, buyParamJson360, v);

            var res = await client.execute<JdUnionOpenGoodsJingfenQueryResponceModel>();
            // var res = await client.execute<object>();

            var queryResult =
                JsonConvert.DeserializeObject<QueryResult>(res.jd_union_open_goods_jingfen_query_responce.queryResult);
            
            return Ok(queryResult);
        }
    }
}