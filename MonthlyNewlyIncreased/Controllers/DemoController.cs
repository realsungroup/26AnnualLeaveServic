using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MonthlyNewlyIncreased.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        [HttpGet]
        public async Task<OkObjectResult> Test(
            [FromQuery] int year,
            [FromQuery] string[] numberIDs
            )
        {
            var annualLeaveResidueResetTask = new AnnualLeaveResidueResetTask();            
            var rsp = await annualLeaveResidueResetTask.Start(year, numberIDs);
            return Ok(rsp);
        }
    }
}
