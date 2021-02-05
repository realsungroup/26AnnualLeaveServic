using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using Newtonsoft.Json.Linq;

namespace MonthlyNewlyIncreased.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncBankCardController : ControllerBase
    {        
        [HttpGet("SyncOne")]
        [HttpPost("SyncOne")]
        public async Task<OkObjectResult> SyncBankCard(
            [FromQuery] string numberID
          )
        {
            if (numberID == null)
            {
                return Ok(new ActionResponseModel { error = -1, message = "没有numberID参数" });
            }
            else
            {
                try
                {
                    var syncBankCardTask = new SyncBankCardTask();
                    return Ok(await syncBankCardTask.SyncBankCard(numberID));
                }
                catch (Exception e)
                {
                    return Ok(new ActionResponseModel { error = -1, message = e.Message });
                }
            }
        }
        [HttpGet("Sync")]
        public async Task<OkObjectResult> SyncBankCards()
        {
            try
            {
                 SyncBankCardJob.start();
                return Ok(new ActionResponseModel { error = 0, message = "任务已启动" });
            }
            catch (Exception e)
            {
                return Ok(new ActionResponseModel { error = -1, message = e.Message });
            }
        }
    }
}
