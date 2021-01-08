using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Jobs;


namespace ShopAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExecuteGroundingJobController : ControllerBase
    {
        /// <summary>
        /// 执行淘宝商品上架任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> executeGroundingJob()
        {
            await GroundingJob.start();
            return Ok(new {error = 0, message = "正在运行"});
        }
    }
}