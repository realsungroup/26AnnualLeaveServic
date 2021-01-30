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
    public class EntryAssignmentController : ControllerBase
    {
        /// <summary>
        /// 执行满足条件的全部员工入职分配
        /// </summary>
        /// <returns></returns>
        [HttpGet("assignmentAll")]
        public async Task<OkObjectResult> AssignmentAll([FromQuery ]string date)
        {
            if (date == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有date参数"});
            }

            try
            {
                EntryAssignmentJob.start(Convert.ToDateTime(date));
                return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
            }
            catch (Exception e)
            {
                return Ok(new ActionResponseModel{error = -1,message = e.Message});
            }
        }
        
        /// <summary>
        /// 执行一个员工入职分配
        /// </summary>
        /// <returns></returns>
        [HttpGet("assignment")]
        public async Task<ActionResult<ActionResponseModel>> Assignment([FromQuery ]string numberID)
        {
            if (numberID == null )
            {
                return new ActionResponseModel{error = -1,message = "没有numberID参数"};
            }
            var newEmployeeTask = new NewEmployeeTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"jobId = '{numberID}'";
            try
            {
                var res = await client.getTable<EmployeeModel>(newEmployeeResid,option);
                if (res.data.Count > 0)
                {
                    var employee = res.data[0];
                    if (employee.totalMonth != null)
                    {
                        var option1 = new GetTableOptionsModal{};
                        option1.cmswhere = $"numberID = '{employee.jobId}' and year = '{employee.enterDate.Substring(0,4)}'";
                        var result = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option1);
                        if (result.data.Count == 0)
                        {
                            await newEmployeeTask.Distribution(res.data[0]);
                            return new ActionResponseModel{error = 0,message = "入职分配成功"};
                        }
                        else
                        {
                            return new ActionResponseModel{error = -1,message = "该员工已有年假季度账户"};
                        }
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}