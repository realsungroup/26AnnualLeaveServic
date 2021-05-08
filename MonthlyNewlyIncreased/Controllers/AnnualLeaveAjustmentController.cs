using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonthlyNewlyIncreased.Http;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Jobs;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using static  MonthlyNewlyIncreased.Constant;

namespace MonthlyNewlyIncreased.Controllers
{
    /// <summary>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AnnualLeaveAjustmentController : ControllerBase
    {
       
        /// <summary>
        /// 按年、季度、人员编号交易
        /// </summary>
        /// <returns></returns>
        [HttpGet("use")]
        [HttpPost("use")]
        public async Task<OkObjectResult> Use([FromQuery] int year,
            [FromQuery] int quarter,[FromQuery] double hours,[FromQuery] int memberId,
            [FromQuery] string type, [FromQuery] string recId )
        {
            if (year == 0 )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有year参数"});
            }
            if (quarter == 0 )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有quarter参数"});
            }
            if (hours == 0 )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有hours参数"});
            }
            if (memberId == 0 )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有memberID参数"});
            }
            if (type == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有type参数"});
            }
            if (recId == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有recId参数"});
            }
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"memberID={memberId} and year={year} and quarter={quarter}";
            try
            {
                var res = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option);
                if (Convert.ToInt32(res.error)  != 0)
                {
                    return Ok(new ActionResponseModel{error = -1,message = res.message});
                }
                if (res.data.Count == 0)
                {
                    return Ok(new ActionResponseModel{error = -1,message = "无此人员编号的年假季度账户"});
                }
                var account = res.data[0];
                //待扣除的年假数
                var waitDeductionDays = hours/8;
                //本次交易的上年剩余额度
                double tradeSNSY = 0;
                //本次交易的上季剩余额度
                double tradeSJSY = 0;
                //本次交易的当季剩余额度
                double tradeDJFP = 0;
                // 上年剩余不够用
                if (account.snsy < waitDeductionDays)
                {
                    tradeSNSY = account.snsy;
                    waitDeductionDays = waitDeductionDays - account.snsy;
                    // 上季剩余不够用
                    if (account.sjsy < waitDeductionDays)
                    {
                        tradeSJSY = account.sjsy;
                        waitDeductionDays = waitDeductionDays - account.sjsy;
                        tradeDJFP = waitDeductionDays;
                    }
                    else
                    {
                        tradeSJSY = waitDeductionDays;
                    }
                }
                else
                {
                    tradeSNSY = waitDeductionDays;
                }                
                List<AnnualLeaveTradeModel> list = new List<AnnualLeaveTradeModel>();
                var trade = new AnnualLeaveTradeModel
                {
                    Type = type,
                    Year = year,
                    Quarter = quarter,
                    snsytrans = tradeSNSY,
                    sjsytrans = tradeSJSY,
                    djfptrans = tradeDJFP,
                    pnid = memberId,
                    NumberID = account.numberID,
                    njsytzjlbh = recId,
                    _state = "added",
                    _id = "1"
                };
                list.Add(trade);
                await client.AddRecords<object>(annualLeaveTradeResid, list);

                var list1 = new List<ModifyAccountModel>();
                list1.Add(new ModifyAccountModel
                {
                    REC_ID = account.REC_ID,
                    _state = "modified",
                    _id=1
                });
                await client.AddRecords<object>(ygnjjdzhResid,list1);
                return Ok(new ActionResponseModel{error = 0,message = "操作成功"});
            }
            catch (Exception e)
            {
                return Ok(new ActionResponseModel{error = -1,message = e.Message});
            }
        }

        /// <summary>
        /// 撤销交易记录
        /// </summary>
        /// <returns></returns>
        [HttpGet("cancelTrade")]
        [HttpPost("cancelTrade")]
        public async Task<OkObjectResult> CancelUse([FromQuery] string recordId,[FromQuery] string type)
        {
            if (recordId == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有recordId参数"});
            }
            if (type == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有type参数"});
            }
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"REC_ID = '{recordId}'";
            try
            {
                var res = await client.getTable<AnnualLeaveTradeModel>(annualLeaveTradeResid,option);
                if (Convert.ToInt32(res.error)  != 0)
                {
                    return Ok(new ActionResponseModel{error = -1,message = res.message});
                }

                if (res.data.Count == 0)
                {
                    return Ok(new ActionResponseModel{error = -1,message = "不存在该交易记录"});
                }

                var record = res.data[0];
                List<AnnualLeaveTradeModel> list = new List<AnnualLeaveTradeModel>();
                var trade = new AnnualLeaveTradeModel
                {
                    Type = type,
                    Year = record.Year,
                    Quarter = record.Quarter,
                    snsytrans = record.snsytrans,
                    sjsytrans = record.sjsytrans,
                    djfptrans = record.djfptrans,
                    pnid = record.pnid,
                    NumberID = record.NumberID,
                    _state = "added",
                    _id = "1"
                };
                list.Add(trade);
                await client.AddRecords<object>(annualLeaveTradeResid, list);
                return Ok(new ActionResponseModel{error = 0,message = "操作成功"});
            }
            catch (Exception e)
            {
                return Ok(new ActionResponseModel{error = -1,message = e.Message});
            }
        }

        /// <summary>
        /// 按年、季度、账户类型、人员编号交易
        /// </summary>
        /// <returns></returns>
        [HttpGet("increase")]
        [HttpPost("increase")]
        public async Task<OkObjectResult> Increase([FromQuery] int year,
            [FromQuery] int quarter,[FromQuery] double hours,
            [FromQuery] int memberId,[FromQuery] string account,
            [FromQuery] string numberId, [FromQuery] string recId)
        {
            if (year == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有year参数"});
            }
            if (quarter == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有quarter参数"});
            }
            if (hours == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有hours参数"});
            }
            if (memberId == 0)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有memberID参数"});
            }
            if (numberId == null)
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有numberId参数"});
            }
            if (account == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有account参数"});
            }
            if (recId == null )
            {
                return Ok(new ActionResponseModel{error = -1,message = "没有recId参数"});
            }
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                var days = hours / 8;
                List<AnnualLeaveTradeModel> list = new List<AnnualLeaveTradeModel>();
                var trade = new AnnualLeaveTradeModel
                {
                    Type = "年假使用减",
                    Year = year,
                    Quarter = quarter,
                    snsytrans = account == "上年剩余余额" ? days : 0,
                    sjsytrans = account == "上季剩余余额" ? days : 0,
                    djfptrans = account == "当季分配余额" ? days : 0,
                    pnid = memberId,
                    NumberID = numberId,
                    njsytzjlbh = recId,
                    _state = "added",
                    _id = "1"
                };
                list.Add(trade);
                await client.AddRecords<object>(annualLeaveTradeResid, list);
                
                var option = new GetTableOptionsModal{};
                option.cmswhere = $"memberID={memberId} and year={year} and quarter={quarter}";
                var res = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option);
                var data = res.data[0];
                var list1 = new List<ModifyAccountModel>();
                list1.Add(new ModifyAccountModel
                {
                    REC_ID = data.REC_ID,
                    _state = "modified",
                    _id = 1
                });
                await client.AddRecords<object>(ygnjjdzhResid,list1);
                return Ok(new ActionResponseModel{error = 0,message = "操作成功"});
            }
            catch (Exception e)
            {
                return Ok(new ActionResponseModel{error = -1,message = e.Message});
            }
        }
    }

    public class ModifyAccountModel
    {
        public string? REC_ID { get; set; }
        //
        public string? _state { get; set; }
        //
        public int? _id{ get; set; }
    }
}