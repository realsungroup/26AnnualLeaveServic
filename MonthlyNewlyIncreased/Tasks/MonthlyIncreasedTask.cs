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
    public class MonthlyIncreasedTask {

        public MonthlyIncreasedTask()
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
        /// 获取到的员工
        /// </summary>
        public List<EmployeeModel> employeeList = new List<EmployeeModel> ();

        /// <summary>
        /// 员工社保天数加1
        /// </summary>
        public async Task<object> IncreaseSocialSecurityMonthly(EmployeeModel employee)
        {
            var list = new List<ModifyEmployeeModel>();
            list.Add(new ModifyEmployeeModel
            {
                REC_ID = employee.REC_ID,
                totalMonth = employee.totalMonth + 1,
                monthAddTrigger = "Y" ,
                _id = 1,
                _state = "modified"
            });
            await client.AddRecords<object>(newEmployeeResid,list);
            return new{};
        }
        
        /// <summary>
        /// 
        /// </summary>
        public async Task<object> Run (string today, int year,string date) {
            var ret = new { };
            var option = new GetTableOptionsModal{};
            option.pageSize = pageSize;
            option.pageIndex = _pageNo;
            if (today == "28")
            {
                option.cmswhere = $"dayNum >= '{today}'";
            }
            else
            {
                option.cmswhere = $"dayNum = '{today}'";
            }
            try {
                var res = await client.getTable<EmployeeModel>(newEmployeeResid,option);
                foreach (var item in res.data)
                {
                    await IncreaseSocialSecurityMonthly(item);
                    if (item.totalMonth!= null)
                    {
                        var total = item.totalMonth + 1;
                        if (total == 12 || total == 120 || total == 240)
                        {
                            if (item.enterDate.Substring(0,7) != date.Substring(0,7))
                            {
                                await Distribution(item,year,date);
                            }
                        }
                    }
                }
                if (HasNextPage(res)) {
                    _pageNo =(Convert.ToInt16(_pageNo) + 1).ToString();
                    await Run(today,year,date);
                }
            } catch (System.Exception exception) {
                Console.WriteLine($"error：{exception}");
            }
            return ret;
        }

        /// <summary>
        /// 给员工分配年假
        /// <param name="employee">员工</param>
        /// </summary>
        public async Task<object> Distribution(EmployeeModel employee,int year,string date)
        {
            var ret = new { };
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            var quarter = GetQuarterByDate(date);
            var quarterDays= getQuarterTradsDays(quarter,date);
            List<AnnualLeaveTradeModel> trades = new List<AnnualLeaveTradeModel>();
            if (1 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[0],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 1,_state = "added",_id = "1"});
            }
            if (2 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[1],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 2,_state = "added",_id = "2"});
            }
            if (3 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[2],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 3,_state = "added",_id = "3"});
            }
            if (4 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[3],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 4,_state = "added",_id = "4"});
            }
            try
            {
                await IncreaseSocialSecurityMonthly(employee);
                //增加4条年假交易记录，类型为‘月度新增’
                await client.AddRecords<object>(annualLeaveTradeResid, trades);
            }
            catch (Exception e)
            {
                AddTaskDetail("月度新增",taskStartTime, DateTime.Now.ToString(datetimeFormatString),e.Message,employee.jobId);
                throw;
            }
            return ret;
        }
        
        /// <summary>
        /// 根据入职日期获取各季度年假数
        /// <param name="currentQuarter">当前季度</param>
        /// <param name="date">日期</param>
        /// </summary>
        public double[] getQuarterTradsDays(int currentQuarter, string date)
        {
            double[] quarterDays = {0,0,0,0};
            int startIndex = currentQuarter - 1;
            //总可用天数
            int totalDays = getConversionDays(date);
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
        /// <param name="conversionDate">折算日期</param>
        /// </summary>
        public int getConversionDays(string conversionDate)
        {
            int days = 5;
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