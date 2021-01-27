using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Http;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using static MonthlyNewlyIncreased.Constant;
using Newtonsoft;
using Newtonsoft.Json;

namespace MonthlyNewlyIncreased.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CreatYearBeginningAndIntoYearLeftController : ControllerBase
    {
        [HttpGet]
        public async Task<OkObjectResult> CreatYearBeginningAndIntoYearLeft(
           [FromQuery] int year
           )
        {
            var creatYearBeginningAndIntoYearLeftTask = new CreatYearBeginningAndIntoYearLeftTask();
            if (year < 1)
            {
                return Ok(new ActionResponseModel { error = -1, message = "年份错误" });
            }
            else
            {
                var rsp = await creatYearBeginningAndIntoYearLeftTask.Start(year);               
                return Ok(new ActionResponseModel { error = 0, message = "年初创建和上年转入执行成功" });
            }
        }

    }
}
