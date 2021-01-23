using System;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonthlyNewlyIncreased.Modals;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;
    
namespace MonthlyNewlyIncreased.Tasks {
    public class QuarterTask {

        public QuarterTask()
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
        private bool HasNextPage (GetTagbleResponseModal<NjjdAccountModal> rsp) {
            if ((Convert.ToInt16(this._pageNo )+1) *  Convert.ToInt16(this.pageSize) > Convert.ToInt16(rsp.total)) {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 任务开始时间
        /// </summary>
        public string taskStartTime;
        
        /// <summary>
        /// </summary>
        public async Task<object> Run (int year, int quarter) {
            var ret = new { };
            var option = new GetTableOptionsModal{};
            option.pageSize = pageSize;
            option.pageIndex = _pageNo;
            option.cmswhere = $"isAlive = 'Y' and year = '{year}' and quarter = '{quarter}'";
            try {
                var res = await this.client.getTable<NjjdAccountModal>(ygnjjdzhResid,option);
                foreach (var item in res.data)
                {
                    try
                    {
                        Console.WriteLine($"----------开始结算--------,工号{item.numberID}---{DateTime.Now.ToString(datetimeFormatString)}");
                        //季度使用
                        await QuarterUse(item);
                        //季度转出
                        await QuarterRollOut(year, quarter, item.numberID);
                        //季度转入
                        await QuarterRollIn(year, quarter, item.numberID);
                        Console.WriteLine($"----------结束结算---------,工号{item.numberID}{DateTime.Now.ToString(datetimeFormatString)}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                if (HasNextPage(res)) {
                    _pageNo =(Convert.ToInt16(_pageNo) + 1).ToString();
                    await Run(year,quarter);
                }
                else
                {
                    Console.WriteLine($"结算完成：{DateTime.Now.ToString(datetimeFormatString)}");
                    AddTask("季度结算", taskStartTime, DateTime.Now.ToString(datetimeFormatString), "");
                }
            } catch (System.Exception exception) {
                Console.WriteLine($"error：{exception}");
                return ret;
            }
            return ret;
        }
        
        /// <summary>
        /// 处理员工季度使用
        ///
        /// 共2个请求
        /// </summary>
        public async Task<object> QuarterUse(NjjdAccountModal account)
        {
            string startTime = DateTime.Now.ToString(datetimeFormatString);
            var year = account.year;
            var quarter = account.quarter;
            var number = account.numberID;
            var isExist = await IsTradeExist("季度使用", year, quarter, number);
            if (!isExist)
            {
                Console.WriteLine("----------开始季度使用---------------");
                try
                {
                    var sum = account.snsy + account.sjsy + account.djfp;
                    var days = await GetLeaveDays(quarter, number, year + "");
                    Console.WriteLine($"待扣年假数：{days}");
                    //三个账户和是否大于或等于待扣除年假数
                    if (sum >= days)
                    {
                        if (days > 0)
                        {
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
                                    //当季分配不够用
                                    if (account.djfp < waitDeductionDays)
                                    {
                                        tradeDJFP = account.djfp;
                                    }
                                    else
                                    {
                                        tradeDJFP = waitDeductionDays;
                                    }
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
                                Type = "季度使用",
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
                            Console.WriteLine($"季度年假账户数据：{JsonConvert.SerializeObject(account)}");
                            Console.WriteLine($"交易数据：{JsonConvert.SerializeObject(trade)}");
                            await client.AddRecords<object>(annualLeaveTradeResid, list);
                        }
                    }
                    else
                    {
                        string endTime = DateTime.Now.ToString(datetimeFormatString);
                        Console.WriteLine("当前季度账户总和{sum}小于季度请的年假数{days}。");
                        //往任务详情表增加一条错误信息，错误信息为当前季度账户总和小于季度清的年假数
                        AddTaskDetail("季度使用", startTime, endTime,
                            $"当前季度账户总和{sum}小于季度请的年假数{days}。年假季度账户信息：{JsonConvert.SerializeObject(account)}", number);
                    }
                }
                catch (Exception e)
                {
                    string endTime = DateTime.Now.ToString(datetimeFormatString);
                    AddTaskDetail("季度使用", startTime, endTime, $"{e.Message}。{year}第{quarter}季度", number);
                    throw;
                }
                Console.WriteLine("----------结束季度使用---------------");
            }
            else
            {
                Console.WriteLine("季度使用已存在");
                string endTime = DateTime.Now.ToString(datetimeFormatString);
                AddTaskDetail("季度使用", startTime, endTime,
                    $"季度使用已存在。", number);
            }
            return new { };
        }
        /// <summary>
        /// 获取员工某年某季度请的年假天数
        /// </summary>
        public async Task<double> GetLeaveDays(int quarter, string number,string year)
        {
            var option = new GetTableOptionsModal{};
            string startMonth = year;
            string endMonth = year;
            switch (quarter)
            {
                case 1:
                    startMonth += "01";
                    endMonth += "03";
                    break;
                case 2:
                    startMonth += "04";
                    endMonth += "06";
                    break;
                case 3:
                    startMonth += "07";
                    endMonth += "09";
                    break;
                case 4:
                    startMonth += "10";
                    endMonth += "12";
                    break;
                default:
                    startMonth += "01";
                    endMonth += "03";
                    break;
            }
            option.cmswhere = $"YGNO = '{number}' and YEARMONTH between '{startMonth}' and '{endMonth}'";
            double days = 0;
            try {
                var res = await this.client.getTable<MonthReportModel>(MonthReportResid,option);
                foreach (var item in res.data)
                {
                    days += item.F_23;
                }
            } catch (System.Exception exception) {
                Console.WriteLine($"GetLeaveDaysError：{exception}");
                throw;
            }
            return days / 8;
        }

        /// <summary>
        /// 处理员工季度转出
        /// <param name="year">年</param>
        /// <param name="quarter">目标季度</param>
        /// <param name="number">工号</param>
        ///
        /// 共3个请求
        /// </summary>
        public async Task<object> QuarterRollOut(int year, int quarter, string number)
        {
            string startTime = DateTime.Now.ToString(datetimeFormatString);
            Console.WriteLine("----------开始季度转出---------------");
            try
            {
                var option = new GetTableOptionsModal{};
                option.cmswhere = $"numberID = '{number}' and year = '{year}' and quarter = '{quarter}'";
                var res = await this.client.getTable<NjjdAccountModal>(ygnjjdzhResid,option);
                if (res.data.Count > 0)
                {
                    var account = res.data[0];
                    var sum = account.snsy + account.sjsy + account.djfp;
                    if (sum > 0)
                    {
                        var trade = new AnnualLeaveTradeModel
                        {
                            Type = "季度转出",
                            Year = year,
                            Quarter = quarter,
                            snsytrans = account.snsy,
                            sjsytrans = account.sjsy,
                            djfptrans = account.djfp,
                            NumberID = number,
                            _state = "added",
                            _id = "1"
                        };
                        List<AnnualLeaveTradeModel> list = new List<AnnualLeaveTradeModel>();
                        list.Add(trade);
                        Console.WriteLine($"季度年假账户数据：{JsonConvert.SerializeObject(account)}");
                        Console.WriteLine($"交易数据：{JsonConvert.SerializeObject(trade)}");
                        await client.AddRecords<object>(annualLeaveTradeResid, list);
                        //锁定季度账户
                        List<ModifyNjjdAccountModel> modifyList = new List<ModifyNjjdAccountModel>();
                        modifyList.Add(new ModifyNjjdAccountModel
                        {
                            REC_ID = account.REC_ID,
                            locked = "Y",
                            _state = "modified",
                            _id = "1"
                        });
                        await client.AddRecords<object>(ygnjjdzhResid, modifyList);
                    }
                } else
                {
                    string endTime = DateTime.Now.ToString(datetimeFormatString);
                    //往任务详情表增加一条错误信息，错误信息为当前季度账户总和小于季度清的年假数
                    AddTaskDetail("季度使用",startTime,endTime,
                        $"工号{number} {year}年{quarter}季度没有年假季度账户",number);
                }
            }
            catch (Exception e)
            {
                string endTime = DateTime.Now.ToString(datetimeFormatString);
                AddTaskDetail("季度转出",startTime,endTime,$"{e.Message}。{year}第{quarter}季度",number);
                throw;
            }
            Console.WriteLine("----------结束季度转出---------------");
            return new { };
        }
        
        /// <summary>
        /// 处理员工季度转入
        /// <param name="year">年</param>
        /// <param name="quarter">目标季度</param>
        /// <param name="number">工号</param>
        ///
        /// 共2个请求
        /// </summary>
        public async Task<object> QuarterRollIn(int year, int quarter, string number)
        {
            string startTime = DateTime.Now.ToString(datetimeFormatString);
            Console.WriteLine("----------开始季度转入---------------");
            try
            {
                var result = await client.getTable<AnnualLeaveTradeModel>(annualLeaveTradeResid,
                    new GetTableOptionsModal
                    {
                        cmswhere = $"Type = '季度转出' and Year = '{year}' and Quarter = '{quarter}' and NumberID = '{number}'"
                    });
                if (quarter != 4)
                {
                    var isExist = await IsTradeExist("季度转入", year,quarter+1, number);
                    if (!isExist)
                    {
                        if (result.data.Count > 0)
                        {
                            var data = result.data[0];
                            var trade = new AnnualLeaveTradeModel
                            {
                                Type = "季度转入",
                                Year = year,
                                Quarter = quarter + 1,
                                snsytrans = data.snsytrans,
                                sjsytrans = data.sjsytrans,
                                djfptrans = data.djfptrans,
                                NumberID = number,
                                _state = "added",
                                _id = "1"
                            };
                            Console.WriteLine($"季度转出数据：{JsonConvert.SerializeObject(data)}");
                            Console.WriteLine($"交易数据：{JsonConvert.SerializeObject(trade)}");
                            List<AnnualLeaveTradeModel> list = new List<AnnualLeaveTradeModel>();
                            list.Add(trade);
                            //await client.AddRecords<object>(annualLeaveTradeResid, list);   
                        }
                        else
                        {
                            WriteLine("没有本季度的转出交易记录");
                            string endTime = DateTime.Now.ToString(datetimeFormatString);
                            AddTaskDetail("季度转入", startTime, endTime, $"没有本季度的转出交易记录。{year}第{quarter}季度", number);
                        }
                    }else
                    {
                        WriteLine("已经存在一条转入交易记录");
                        string endTime = DateTime.Now.ToString(datetimeFormatString);
                        AddTaskDetail("季度转入",startTime,endTime,$"已经存在一条转入交易记录。{year}第{quarter}季度",number);
                    }
                }
                else
                {
                    if (result.data.Count > 0)
                    {
                        var yearleft = await client.getTable<AnnualLeaveTradeModel>(YearLeftResid,
                            new GetTableOptionsModal
                            {
                                cmswhere = $"Quarter = '{year+1}' and NumberID = '{number}'"
                            });
                        if (yearleft.data.Count==0)
                        {
                            var trade = result.data[0];
                            //第四季度
                            var yearLeftList = new List<YearLeftModel>();
                            var yearLeftData = new YearLeftModel
                            {
                                NumberID = number,
                                Quarter = year + 1,
                                Residue = trade.sjsytrans + trade.djfptrans,
                                _state = "added",
                                _id = "1",
                            };
                            yearLeftList.Add(yearLeftData);
                            //往年假上年剩余明细表增加一条记录
                            await client.AddRecords<object>(YearLeftResid, yearLeftList);
                        }
                        else
                        {
                            WriteLine("已经存在一条上年剩余记录");
                            string endTime = DateTime.Now.ToString(datetimeFormatString);
                            AddTaskDetail("季度转入",startTime,endTime,$"已经存在一条上年剩余记录。",number);
                        }
                    }else
                    {
                        WriteLine("没有本季度的转出交易记录");
                        string endTime = DateTime.Now.ToString(datetimeFormatString);
                        AddTaskDetail("季度转入",startTime,endTime,$"没有本季度的转出交易记录。{year}第{quarter}季度",number);
                    }
                }
            }
            catch (Exception e)
            {
                string endTime = DateTime.Now.ToString(datetimeFormatString);
                AddTaskDetail("季度转入",startTime,endTime,$"{e.Message}。{year}第{quarter}季度",number);
                throw;
            }
            Console.WriteLine("----------结束季度转入---------------");
            return new { };
        }
    }
}