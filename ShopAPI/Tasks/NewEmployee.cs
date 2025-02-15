using System;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using static ShopAPI.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using FastJSON;
using Newtonsoft.Json;
using ShopAPI.Http;
using static System.Console;
using ShopAPI.Modals;

namespace ShopAPI.Tasks {
    public class NewEmployee {

        public NewEmployee()
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
        private bool HasNextPage (GetTagbleResponseModal<EmployeeModal> rsp) {
            if ((Convert.ToInt16(this._pageNo )+1) *  Convert.ToInt16(this.pageSize) > Convert.ToInt16(rsp.total)) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取到的员工
        /// </summary>
        public List<EmployeeModal> employeeList = new List<EmployeeModal> ();

        /// <summary>
        /// 获取7天内入职的员工
        /// </summary>
        public async Task<object> GetNewEmployeeList () {
            var ret = new { };
            var option = new GetTableOptionsModal{};
            option.pageSize = pageSize;
            option.pageIndex = _pageNo;
            option.cmswhere = $"enterDate >= '{DateTime.Now.AddDays(-7).ToString(dateFormatString)}' and enterDate <= '{DateTime.Now.ToString(dateFormatString)}'";
            try {
                var res = await this.client.getTable<EmployeeModal>(newEmployeeResid,option);
                foreach (var item in res.data)
                {
                    var option1 = new GetTableOptionsModal{};
                    option1.cmswhere = $"numberID = '{item.jobId}'";
                    var result = await this.client.getTable<NjjdAccountModal>(ygnjjdzhResid,option1);
                    // Console.WriteLine(JSON.ToJSON(result));
                    if (result.data.Count == 0)
                    {
                        employeeList.Add(item);
                    }
                }
                if (HasNextPage(res)) {
                    _pageNo =(Convert.ToInt16(_pageNo) + 1).ToString();
                    await GetNewEmployeeList();
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
        public async Task<object> Distribution(string id)
        {
            var ret = new { };
            try
            {
                var option = new GetTableOptionsModal{};
                option.cmswhere = $"jobId = '{id}'";
                var res = await this.client.getTable<EmployeeModal>(newEmployeeResid,option);
                if (res.data.Count > 0)
                {
                    var data = res.data[0];
                    if (res.data[0].serviceAge != null)
                    {
                        var year = DateTime.Today.Year;
                        var month = Convert.ToDateTime(data.enterDate).Month-1;
                        var quarter = month / 3 + 1;
                        var  quarterDays= this.getQuarterTradsDays((int)data.serviceAge, quarter,data.enterDate);
                        
                         List<NjjdAccountModal> accounts = new List<NjjdAccountModal>();
                         accounts.Add(new NjjdAccountModal{numberID = id,year = year,quarter = 1,_state = "added",_id = "1"});
                         accounts.Add(new NjjdAccountModal{numberID = id,year = year,quarter = 2,_state = "added",_id = "2"});
                         accounts.Add(new NjjdAccountModal{numberID = id,year = year,quarter = 3,_state = "added",_id = "3"});
                         accounts.Add(new NjjdAccountModal{numberID = id,year = year,quarter = 4,_state = "added",_id = "4"});
                         List<AnnualLeaveTradeModal> trades = new List<AnnualLeaveTradeModal>();
                         trades.Add(new AnnualLeaveTradeModal{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[0],Type = "入职分配",NumberID = id,Year = year,Quarter = 1,_state = "added",_id = "1"});
                         trades.Add(new AnnualLeaveTradeModal{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[1],Type = "入职分配",NumberID = id,Year = year,Quarter = 2,_state = "added",_id = "2"});
                         trades.Add(new AnnualLeaveTradeModal{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[2],Type = "入职分配",NumberID = id,Year = year,Quarter = 3,_state = "added",_id = "3"});
                         trades.Add(new AnnualLeaveTradeModal{snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[3],Type = "入职分配",NumberID = id,Year = year,Quarter = 4,_state = "added",_id = "4"});
                         Console.WriteLine(JsonConvert.SerializeObject(trades));
                         try
                         {
                             //创建4条季度年假账户
                             await this.client.AddRecords<object>(ygnjjdzhResid, accounts);
                             //增加4条年假交易记录，类型为‘入职分配’
                             await this.client.AddRecords<object>(annualLeaveTradeResid, trades);
                         }
                         catch (Exception e)
                         {
                             WriteLine(e);
                             throw;
                         }
                    }
                }                
            }
            catch (Exception e)
            {
                WriteLine(e);
                return ret;
            }
            return ret;
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
            int totalDays = getConversionDays(workyears,date,true);
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
            Console.WriteLine($"quarterDays:{JSON.ToJSON(quarterDays)}");
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