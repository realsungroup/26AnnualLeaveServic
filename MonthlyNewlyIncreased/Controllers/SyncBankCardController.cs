using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;

namespace MonthlyNewlyIncreased.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncBankCardController : ControllerBase
    {
        [HttpGet("SyncOne")]
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
                var syncBankCardTask = new SyncBankCardTask();
                return Ok(await syncBankCardTask.SyncBankCards());
            }
            catch (Exception e)
            {
                return Ok(new ActionResponseModel { error = -1, message = e.Message });
            }
        }
    }
}
