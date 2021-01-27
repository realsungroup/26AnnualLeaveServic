using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using MonthlyNewlyIncreased.Http;
using static MonthlyNewlyIncreased.Constant;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Controllers
{
    /// <summary>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MonthUseController : ControllerBase
    {
        /// <summary>
        /// 执行全部员工的月度使用
        /// </summary>
        /// <returns></returns>
        [HttpGet("useALl")]
        public async Task<OkObjectResult> UseALl([FromQuery ]string yearmonth)
        {
            if (yearmonth == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有yearmonth参数"});
            }
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            Console.WriteLine("开始执行月度使用：" + taskStartTime);
            var monthUseTask = new MonthUseTask();
            monthUseTask.taskStartTime = taskStartTime;
            var month = Convert.ToDateTime(yearmonth);
            var quarter = GetQuarterByMonth(month.Month);
            monthUseTask.Run(month.Year,quarter,month.ToString("MM"));           
            return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
        }
        
        /// <summary>
        /// 执行一个月度员工
        /// </summary>
        /// <returns></returns>
        [HttpGet("use")]
        public async Task<ActionResult<ActionResponseModel>> Assignment(
            [FromQuery ]string numberID,
            [FromQuery ]string yearmonth
        )
        {
            if (numberID == null )
            {
                return new ActionResponseModel{error = -1,message = "没有numberID参数"};
            }
            if (yearmonth == null )
            {
                return new ActionResponseModel{error = -1,message = "没有yearmonth参数"};
            }
            var monthUseTask = new MonthUseTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                var option1 = new GetTableOptionsModal();
                var yearMonth = Convert.ToDateTime(yearmonth);
                var quarter = GetQuarterByMonth(yearMonth.Month);
                option1.cmswhere = $"numberID = '{numberID}' and quarter = '{quarter}' and year = '{yearMonth.Year}'";
                var result = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option1);
                if (result.data.Count > 0)
                {
                    await monthUseTask.MonthUse(result.data[0], yearMonth.ToString("yyyyMM") );
                    return new ActionResponseModel{error = 0,message = "月度使用成功"};
                }
                else
                {                   
                    return new ActionResponseModel{error = 0,message = "该员工没有年假季度账户"};
                }
            }
            catch (Exception e)
            {
                return new ActionResponseModel{error = -1,message = e.Message};
            }
        }

       
    }
}