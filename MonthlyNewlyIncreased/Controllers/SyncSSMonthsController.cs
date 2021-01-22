using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;

namespace MonthlyNewlyIncreased.Controllers
{
    /// <summary>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SyncSSMonthsController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("sync")]
        public async Task<OkObjectResult> Sync()
        {
            var monthlyIncreasedTask = new SyncSocialSecurityMonthsTask();
            monthlyIncreasedTask.GetNewEmployeeList();
            return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
        }
        
    }
}