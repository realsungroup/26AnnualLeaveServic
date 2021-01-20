using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Tasks;

namespace MonthlyNewlyIncreased.Controllers
{
    /// <summary>
    /// 获取 realsun 平台的 accessToken
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> Test()
        {
            var today = DateTime.Today.ToString("MM-dd");
            var monthlyIncreasedTask = new MonthlyIncreasedTask();
            await  monthlyIncreasedTask.GetNewEmployeeList();
            foreach (var item in monthlyIncreasedTask.employeeList)
            {
                var enterDate = item.enterDate.Substring(item.enterDate.Length - 5);
                var month = Convert.ToDateTime(item.enterDate).Month-1;
                var quarter = month / 3 + 1;
                var  quarterDays=  monthlyIncreasedTask.getQuarterTradsDays(quarter,item.enterDate);
                //社龄是否增加
                if ((enterDate == today) && (item.serviceAge != null))
                {
                    int workingyears = (int)item.serviceAge + 1;
                    //增加后的社龄是否为1，10，20
                    if (workingyears==1||workingyears==10||workingyears==20)
                    {
                        
                    }
                }
            }
            return Ok(new {});
        }
    }
}