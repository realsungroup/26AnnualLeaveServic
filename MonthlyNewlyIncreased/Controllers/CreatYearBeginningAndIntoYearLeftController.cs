using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Http;
using MonthlyNewlyIncreased.Tasks;

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
            var rsp = await creatYearBeginningAndIntoYearLeftTask.Start(year);
            return Ok(rsp);
        }

    }
}
