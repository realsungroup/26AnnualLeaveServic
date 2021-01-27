using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MonthlyNewlyIncreased.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AnnualLeaveResidueResetController : ControllerBase
    {
        [HttpGet]
        public async Task<OkObjectResult> AnnualLeaveResidueReset(
            [FromQuery] int year,
            [FromQuery] string[] numberIDs
            )
        {
            var annualLeaveResidueResetTask = new AnnualLeaveResidueResetTask();
            if (year < 1)
            {
                return Ok(new ActionResponseModel { error = -1, message = "年份错误" });
            }
            else
            {
                var rsp = await annualLeaveResidueResetTask.Start(year, numberIDs);
                return Ok(new ActionResponseModel { error = 0, message = "剩余清零成功" });
            }
        }
    }
}
