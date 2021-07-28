using System;

using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Modals;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Tasks {
    public class NewEmployeeTask {

        public NewEmployeeTask()
        {
            this.client = new LzRequest(realsunBaseURL);
            this.client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        private LzRequest client = null;

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
        private bool HasNextPage (GetTagbleResponseModal<EmployeeModel> rsp) {
            if ((Convert.ToInt16(this._pageNo )+1) *  Convert.ToInt16(this.pageSize) > Convert.ToInt16(rsp.total)) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// </summary>
        public async Task<object> Run (string cmswhere) {
            var ret = new { };
            var option = new GetTableOptionsModal{};
            option.pageSize = pageSize;
            option.pageIndex = _pageNo;
            option.cmswhere = cmswhere;
            try {
                var res = await this.client.getTable<EmployeeModel>(ALL_NEW_EMPLOYEE,option);
                foreach (var item in res.data)
                {
                    var option1 = new GetTableOptionsModal{};
                    option1.cmswhere = $"NumberID = '{item.jobId}' and year = '{item.enterDate.Substring(0,4)}' and Type = '入职分配'";
                    var result = await this.client.getTable<NjjdAccountModal>(annualLeaveTradeResid,option1);
                    if (result.data.Count == 0)
                    {
                        await Distribution(item);
                    }
                }
                if (HasNextPage(res)) {
                    _pageNo =(Convert.ToInt16(_pageNo) + 1).ToString();
                    await Run(cmswhere);
                } else {
                    WriteLine ("  over...");
                }
            } catch (System.Exception exception) {
                Console.WriteLine($"error：{exception}");
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// 给新员工分配年假
        /// <param name="id">员工工号</param>
        /// </summary>
        public async Task<object> Distribution(EmployeeModel employee)
        {
            var ret = new { };
            string startTime = DateTime.Now.ToString(datetimeFormatString);
            try
            {
                if (employee.wxMonths != null)
                {
                    var year = Convert.ToDateTime(employee.enterDate).Year;
                    var quarter = GetQuarterByDate(employee.enterDate);
                    var currentQuarter = GetQuarterByDate(DateTime.Today.ToString("yyyy-MM-dd"));
                    var quarterDays= this.getQuarterTradsDays((int)employee.beforeServiceAge, quarter,employee.enterDate);
                    var jobid = employee.jobId;
                    var diff = currentQuarter - quarter;
                    double surplus = 0;
                    while (diff>0)
                    {
                        surplus += quarterDays[currentQuarter - diff-1];
                        quarterDays[currentQuarter - diff - 1] = 0.0;
                        diff--;
                    }
                    quarterDays[currentQuarter-1] += surplus; 
                    List<AnnualLeaveTradeModel> trades = new List<AnnualLeaveTradeModel>();
                    trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[0],Type = "入职分配",NumberID = jobid,Year = year,Quarter = 1,_state = "added",_id = "1"});
                    trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[1],Type = "入职分配",NumberID = jobid,Year = year,Quarter = 2,_state = "added",_id = "2"});
                    trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[2],Type = "入职分配",NumberID = jobid,Year = year,Quarter = 3,_state = "added",_id = "3"});
                    trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[3],Type = "入职分配",NumberID = jobid,Year = year,Quarter = 4,_state = "added",_id = "4"});
                    //增加4条年假交易记录，类型为‘入职分配’
                    await client.AddRecords<object>(annualLeaveTradeResid, trades);
                }
                else
                {
                    string endTime = DateTime.Now.ToString(datetimeFormatString);
                    AddTaskDetail("入职分配",startTime,endTime,
                        $"工号{employee.jobId}没有社保月数",employee.jobId);
                }
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// 根据入职日期获取各季度年假数
        /// <param name="workyears">员工社会工龄</param>
        /// <param name="currentQuarter">当前季度</param>
        /// </summary>
        public double[] getQuarterTradsDays(int workyears, int currentQuarter, string date)
        {
            double[] quarterDays = {0,0,0,0};
            int startIndex = currentQuarter - 1;
            //总可用天数
            int totalDays = getConversionDays(workyears,date);
            Console.WriteLine($"总年假天数:{totalDays}");
            double leftDays = totalDays;
            //每季度平均天数
            float avg =(float) totalDays / (4 - startIndex);
            Console.WriteLine($"平均每季度分配年假数{avg}");
            while (startIndex<4)
            {
                //平均数的整数位
                int integer = (int) avg;
                //平均数的小数位
                double _decimal = avg - (double) integer;
                if (_decimal > 0.5)
                {
                    integer++;
                    _decimal = 0;
                }
                else
                {
                    if (_decimal>0)
                    {
                        _decimal = 0.5;
                    }
                }
                double day;
                if (leftDays>avg)
                {
                     day= integer + _decimal;
                }
                else
                {
                    day = leftDays;
                }
                leftDays = leftDays - day;
                Console.WriteLine($"q{startIndex+1}分配的年假数:{day}------------剩余可分配年假数:{leftDays}");
                quarterDays[startIndex] = day;
                startIndex++;
            }
            return quarterDays;
        }

        /// <summary>
        /// 根据社龄和折算日期获取折算后的年假天数
        /// <param name="workyears">员工社会工龄</param>
        /// <param name="conversion">折算日期</param>
        /// </summary>
        public int getConversionDays(int workyears, string conversionDate,bool isConversion=true)
        {
            int days = 0;
            if (workyears<10 && workyears>=1)
            {
                days= 5;
            }
            if (workyears<20 && workyears>=10)
            {
                days= 10;
            }
            if (workyears>=20)
            {
                days= 15;
            }

            if (!isConversion)
            {
                return days;
            }
            DateTime t1 = Convert.ToDateTime(conversionDate);
            int year = t1.Year;
            DateTime tbase = Convert.ToDateTime(string.Format("{0}-1-1",year));
            TimeSpan ts = t1 - tbase;
            int d = ts.Days;
            int difference = 365 - d;
            float percent = (float) difference / 365;
            int daysConversion =(int) (percent * days);
            return daysConversion;
        }
    }
}