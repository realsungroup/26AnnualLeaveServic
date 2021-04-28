using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Http;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using static  MonthlyNewlyIncreased.Constant;

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
            SyncSocialSecurityMonthsJob.start();
            return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("syncOne")]
        [HttpPost("syncOne")]
        public async Task<OkObjectResult> SyncOne([FromQuery] string numberID)
        {
            if (numberID == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有numberID参数"});
            }
            var syncSocialSecurityMonthsTask = new SyncSocialSecurityMonthsTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"jobId = '{numberID}'";
            try
            {
                var res = await client.getTable<EmployeeModel>(newEmployeeResid,option);
                if (Convert.ToInt32(res.error)  != 0)
                {
                    return Ok(new ActionResponseModel{error = -1,message = res.message});
                }
                if (res.data?.Count > 0)
                {
                    await syncSocialSecurityMonthsTask.SyncMonths(res.data[0]);
                    return Ok(new ActionResponseModel{error = 0,message = "已同步社保信息"});
                }
                else
                {
                    return Ok(new ActionResponseModel{error = -1,message = "内网后台没有该员工"});
                }
            }
            catch (Exception e)
            {
                return Ok(new ActionResponseModel{error = -1,message = e.Message});
            }
        }
    }
}