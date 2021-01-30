using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using MonthlyNewlyIncreased.Http;
using static MonthlyNewlyIncreased.Constant;

namespace MonthlyNewlyIncreased.Controllers
{
    /// <summary>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MonthlyIncreaseController : ControllerBase
    {
        /// <summary>
        /// 执行满足条件的全部员工月度新增
        /// </summary>
        /// <returns></returns>
        [HttpGet("increaseAll")]
        public async Task<OkObjectResult> IncreaseAll([FromQuery ]string date)
        {
            if (date == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有date参数"});
            }
            try
            {
                var datetime = Convert.ToDateTime(date);
                if (datetime.Day > 28)
                {
                    return Ok(new ActionResponseModel{error = -1,message = "请传入1-28号的日期，因为大于28号的会在28号统一进行结算。"});
                }
                else
                {
                    MonthlyIncreasedJob.start(datetime);
                    return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
                }
            }
            catch (Exception e)
            {                
                return Ok(new ActionResponseModel{error = -1,message = e.Message});
            }
        }
        
        /// <summary>
        /// 执行一个员工月度新增
        /// </summary>
        /// <returns></returns>
        [HttpGet("increase")]
        [HttpPost("increase")]
        public async Task<ActionResult<ActionResponseModel>> Increase(
            [FromQuery ]string numberID,
            [FromQuery ]string date)
        {
            if (numberID ==null)
            {
                return new ActionResponseModel{error = -1,message = "没有numberID参数"};
            }
            if (date == null )
            {
                return new ActionResponseModel{error = -1,message = "没有date参数"};
            }
            var monthlyIncreasedTask = new MonthlyIncreasedTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal();
            option.cmswhere = $"jobId = '{numberID}'";
            var res = await client.getTable<EmployeeModel>(newEmployeeResid,option);
            Console.WriteLine("月度新增");
            if (res.data.Count > 0)
            {
                var employee = res.data[0];
                if (employee.totalMonth != null)
                {
                    var year = DateTime.Today.Year;
                    await monthlyIncreasedTask.Distribution(res.data[0], year, date);
                    return new ActionResponseModel {error = 0, message = "月度新增已完成"};
                }
                else
                {
                    return new ActionResponseModel{error = -1,message = "该员工社保月数为空"};
                }
            }
            else
            {
                return new ActionResponseModel{error = -1,message = "没有该员工"};
            }
        }
    }
}