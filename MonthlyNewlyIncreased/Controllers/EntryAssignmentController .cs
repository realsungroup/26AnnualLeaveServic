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
        public async Task<OkObjectResult> AssignmentAll()
        {
            var newEmployeeTask = new NewEmployeeTask();
            await  newEmployeeTask.GetNewEmployeeList();
            foreach (var item in newEmployeeTask.employeeList)
            {
                await newEmployeeTask.Distribution(item);
            }
            return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
        }
        
        /// <summary>
        /// 执行一个员工入职分配
        /// </summary>
        /// <returns></returns>
        [HttpGet("assignment")]
        public async Task<ActionResult<ActionResponseModel>> Assignment([FromQuery ]string numberID)
        {
            if (Convert.ToBoolean(numberID) )
            {
                return new ActionResponseModel{error = -1,message = "没有numberID参数"};
            }
            var newEmployeeTask = new NewEmployeeTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"numberID = '{numberID}'";
            var res = await client.getTable<EmployeeModel>(newEmployeeResid,option);
            if (res.data.Count > 0)
            {
                await newEmployeeTask.Distribution(res.data[0]);
            }
            return new ActionResponseModel{error = 0,message = ""};
        }
    }
}