using System;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Tasks {
    public class MonthUseTask {

        public MonthUseTask()
        {
            client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            wxclient = new LzRequest(WXBaseURL);
            wxclient.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        private LzRequest client = null;
        private LzRequest wxclient = null;

        /// <summary>
        /// 页码
        /// </summary>
        private string _pageNo = "0";

        /// <summary>
        /// 每页数量
        /// </summary>
        private string pageSize = "100";
        
        /// <summary>
        /// 是否还有下一页数据
        /// </summary>
        /// <param name="rsp"></param>
        /// <returns></returns>
        private bool HasNextPage (GetTagbleResponseModal<NjjdAccountModal> rsp)
        {
            return false;
            if ((Convert.ToInt16(_pageNo )+1) *  Convert.ToInt16(pageSize) > Convert.ToInt16(rsp.total)) {
                return false;
            }
            return true;
        }

        public string taskStartTime;
        /// <summary>
        /// </summary>
        public async Task<object> Run (int year,int quarter,string month) {
            var ret = new { };
            var option = new GetTableOptionsModal{};
            option.pageSize = pageSize;
            option.pageIndex = _pageNo;
            option.cmswhere = $"isAlive = 'Y' and year = '{year}' and quarter = '{quarter}'";
            try {
                var res = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option);
                foreach (var item in res.data)
                {
                    await MonthUse(item, month);
                }
                if (HasNextPage(res)) {
                    _pageNo =(Convert.ToInt16(_pageNo) + 1).ToString();
                    await Run(year,quarter,month);
                }
                else
                {
                    AddTask("月度使用", taskStartTime, DateTime.Now.ToString(datetimeFormatString), "");
                    WriteLine($"结束执行月度使用：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                }
            } catch (Exception exception) {
                WriteLine($"error：{exception}");
                return ret;
            }
            return ret;
        }
        
        /// <summary>
        /// 获取员工某年某月请的年假天数
        /// </summary>
        public async Task<double> GetLeaveDays(string number,string yearmonth)
        {
            var option = new GetTableOptionsModal{};
           
            option.cmswhere = $"YGNO = '{number}' and YEARMONTH = '{yearmonth}'";
            double days = 0;
            try {
                var res = await client.getTable<MonthReportModel>(MonthReportResid,option);
                foreach (var item in res.data)
                {
                    days += item.F_23;
                }
            } catch (Exception exception) {
                WriteLine($"GetLeaveDaysError：{exception}");
                throw;
            }
            return days / 8;
        }

        /// <summary>
        /// 处理员工季度使用
        ///
        /// 共3个请求
        /// </summary>
        public async Task<bool> MonthUse(NjjdAccountModal account,string yearmonth)
        {
            string startTime = DateTime.Now.ToString(datetimeFormatString);
            var year = account.year;
            var quarter = account.quarter;
            var number = account.numberID;
            WriteLine("----------开始月度使用---------------");
            try
            {
                var days = await GetLeaveDays( number,  yearmonth);
                WriteLine($"待扣年假数：{days}");
                //待扣除的年假数
                var waitDeductionDays = days;
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
                    Type = "月度使用",
                    Year = year,
                    Quarter = quarter,
                    snsytrans = tradeSNSY,
                    sjsytrans = tradeSJSY,
                    djfptrans = tradeDJFP,
                    NumberID = number,
                    _state = "added",
                    _id = "1"
                };
                list.Add(trade);
                WriteLine($"季度年假账户数据：{JsonConvert.SerializeObject(account)}");
                WriteLine($"交易数据：{JsonConvert.SerializeObject(trade)}");
                await client.AddRecords<object>(annualLeaveTradeResid, list);
                WriteLine("----------结束月度使用---------------");
                return true;
            }
            catch (Exception e)
            {
                string endTime = DateTime.Now.ToString(datetimeFormatString);
                AddTaskDetail("月度使用", startTime, endTime, $"{e.Message}。{year}第{quarter}季度", number);
                return false;
            }
        }
    }
}