using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Jobs;


namespace ShopAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExecuteUndercarriageJDJobController : ControllerBase
    {
        /// <summary>
        /// 执行京东商品下架任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> executeUndercarriageJDJob()
        {
            var res = await UndercarriageJDJob.start();
            return Ok(res);
        }
    }
}