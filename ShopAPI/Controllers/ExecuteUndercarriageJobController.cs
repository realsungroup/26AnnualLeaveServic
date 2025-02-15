using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Jobs;


namespace ShopAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExecuteUndercarriageJobController : ControllerBase
    {
        /// <summary>
        /// 执行淘宝商品下架任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> executeUndercarriageJob()
        {
            var res = await UndercarriageJob.start();
            return Ok(res);
        }
    }
}