using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Modals;
using MonthlyNewlyIncreased.Tasks;
using MonthlyNewlyIncreased.Http;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;
using static MonthlyNewlyIncreased.Constant;

namespace MonthlyNewlyIncreased.Controllers
{
    /// <summary>
    /// 季度结转
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuarterSmeettlentController : ControllerBase
    {
        /// <summary>
        /// 单个员工季度结算
        /// </summary>
        /// <returns></returns>
        [HttpGet("settlement")]
        public async Task<OkObjectResult> Settlement(
            [FromQuery]string employeeNumber,
            [FromQuery]string employeeNumberColumn,
            [FromQuery]string employeeResid,
            [FromQuery]string numberID,
            [FromQuery]int year,
            [FromQuery]int quarter
        )
        {
            if (numberID == null)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有numberID参数"});
            }
            if (year == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有year参数"});
            }
            if (quarter == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有quarter参数"});
            }
            var task = new QuarterTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal();
            option.cmswhere = $"numberID = '{numberID}' and year = '{year}' and quarter = '{quarter}'";
            var res = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option);
            if (res.data.Count > 0)
            {
                var account = res.data[0];
                //季度使用
                await task.QuarterUse(account);
                //季度转出
                await task.QuarterRollOut(year, quarter, numberID);
                //季度转入
                await task.QuarterRollIn(year, quarter, numberID);
                return Ok(new ActionResponseModel{error = 0,message = ""});
            }
            else
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有年假季度账户"});
            }
        }
        
        /// <summary>
        /// 单个员工季度使用
        /// </summary>
        /// <returns></returns>
        [HttpGet("use")]
        public async Task<OkObjectResult> Use(
            [FromQuery]string numberID,
            [FromQuery]int year,
            [FromQuery]int quarter
        )
        {
            if ( numberID == null)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有numberID参数"});
            }
            if (year == 0 )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有year参数"});
            }
            if (quarter== 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有quarter参数"});
            }
            var task = new QuarterTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal();
            option.cmswhere = $"year = '{year}' and quarter = '{quarter}'";
            var res = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option);
            if (res.data.Count > 0)
            {
                var account = res.data[0];
                //季度使用
                await task.QuarterUse(account);
            }
            return Ok(new ActionResponseModel{error = 0,message = ""});
        }
        /// <summary>
        /// 单个员工季度转出
        /// </summary>
        /// <returns></returns>
        [HttpGet("rollout")]
        public async Task<OkObjectResult> Rollout(
            [FromQuery]string numberID,
            [FromQuery]int year,
            [FromQuery]int quarter
        )
        {
            if ( numberID == null)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有numberID参数"});
            }
            if (year == 0 )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有year参数"});
            }
            if (quarter== 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有quarter参数"});
            }
            var task = new QuarterTask();
            //季度转出
            await task.QuarterRollOut(year, quarter, numberID);
            return Ok(new ActionResponseModel{error = 0,message = ""});
        }
        /// <summary>
        /// 单个员工季度转入
        /// </summary>
        /// <returns></returns>
        [HttpGet("rollin")]
        public async Task<OkObjectResult> Rollin(
            [FromQuery]string numberID,
            [FromQuery]int year,
            [FromQuery]int quarter
        )
        {
            if ( numberID == null)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有numberID参数"});
            }
            if (year == 0 )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有year参数"});
            }
            if (quarter== 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有quarter参数"});
            }
            var task = new QuarterTask();
            //季度转入
            await task.QuarterRollIn(year, quarter, numberID);
            return Ok(new ActionResponseModel{error = 0,message = ""});
        }
        
        /// <summary>
        /// 所有员工季度结算
        /// </summary>
        /// <returns></returns>
        [HttpGet("settlementAll")]
        public async Task<OkObjectResult> SettlementAll(
            [FromQuery]string employeeNumber,
            [FromQuery]string employeeNumberColumn,
            [FromQuery]string employeeResid,
            [FromQuery]int year,
            [FromQuery]int quarter
        )
        {
            if (year == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有year参数"});
            }
            if (quarter == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有quarter参数"});
            }
            var task = new QuarterTask();
            task.taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            task.Run(year, quarter);
            return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
        }
    }
}