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
        public async Task<OkObjectResult> IncreaseAll()
        {
            var monthlyIncreasedTask = new MonthlyIncreasedTask();
            await  monthlyIncreasedTask.GetNewEmployeeList();
            foreach (var item in monthlyIncreasedTask.employeeList)
            {
                await monthlyIncreasedTask.Distribution(item);
            }
            return Ok(new ActionResponseModel{error = 0,message = "任务已启动"});
        }
        
        /// <summary>
        /// 执行一个员工月度新增
        /// </summary>
        /// <returns></returns>
        [HttpGet("increase")]
        public async Task<ActionResult<ActionResponseModel>> Increase([FromQuery ]string numberID)
        {
            if (Convert.ToBoolean(numberID) )
            {
                return new ActionResponseModel{error = -1,message = "没有numberID参数"};
            }
            var monthlyIncreasedTask = new MonthlyIncreasedTask();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"numberID = '{numberID}'";
            var res = await client.getTable<EmployeeModel>(newEmployeeResid,option);
            if (res.data.Count > 0)
            {
                await monthlyIncreasedTask.Distribution(res.data[0]);
            }
            return new ActionResponseModel{error = 0,message = ""};
        }
    }
}