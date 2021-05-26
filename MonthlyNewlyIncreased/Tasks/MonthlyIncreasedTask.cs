using System;
using System.Collections;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using Newtonsoft.Json.Linq;
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
        /// 保存员工数据
        /// </summary>
        public async Task<JToken> SaveEmployee(EmployeeModel employee)
        {
            var list = new List<ModifyEmployeeModel>();
            list.Add(new ModifyEmployeeModel
            {
                REC_ID = employee.REC_ID,
                monthAddTrigger = "Y" ,
                _id = 1,
                _state = "modified"
            });
            var res = await client.AddRecords<Hashtable>(newEmployeeResid,list);
            JArray data = (JArray)res["data"];
            return data[0];
        }
        
        /// <summary>
        /// 
        /// </summary>
        public async Task<object> Run (int year,string date) {
            var ret = new { };
            try {
                var res = await client.getTable<EmployeeModel>(newEmployeeResid);
                foreach (var item in res.data)
                {
                    var option1 = new GetTableOptionsModal
                    {
                        cmswhere = $"memberID={item.personId} and year={year}"
                    };
                    var savedData = await SaveEmployee(item);
                    var total = Convert.ToInt32( savedData["newTotalMonth"]);
                    var accountsRes = await client.getTable<NjjdAccountModal>(ygnjjdzhResid,option1);
                    if (accountsRes.data.Count > 0)
                    {
                        if (total == 12 || total == 120 || total == 240)
                        {
                            var exist = await IsTradeExist("月度新增", year, item.personId);
                            var serviceMonths = Convert.ToInt32(savedData["serviceMonths"]);
                            if (!exist &&  serviceMonths> 0)
                            {
                                await Distribution(item,year,date,serviceMonths);
                            }
                        }
                    }
                    else
                    {
                        await CreateAccountAndDistribution(item,year,total);
                    }
                }
            } catch (Exception exception) {
                WriteLine($"error：{exception}");
            }
            return ret;
        }
        /// <summary>
        /// 判断交易是否已经产生
        /// </summary>
        /// <param name="type">类型 </param>
        /// <param name="year">年</param>
        /// <param name="number">工号</param>
        /// <returns></returns>
        /// 
        public static async Task<bool> IsTradeExist(string type,int year,int number)
        {
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                var result =
                    await client.getTable<AnnualLeaveTradeModel>(annualLeaveTradeResid,
                        new GetTableOptionsModal
                        {
                            cmswhere = $"Type = '{type}' and Year = '{year}' and pnid={number}"
                        });
                bool isExist = result.data.Count > 0;
                return isExist;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        
        /// <summary>
        /// 创建账户并分配年假
        /// </summary>
        /// <param name="employee">员工</param>
        /// <param name="year">年</param>
        /// <param name="months">社保月数</param>
        /// <returns></returns>
        /// 
        public  async Task<Object> CreateAccountAndDistribution(EmployeeModel employee, int year, int months)
        {
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                //创建账户
                await CreateAccount(employee,year);
                //分配年假
                await BeginningOfYearCreate(employee, year);
                //上年转入
                await TransferredLastYear(employee, year);
                //如果正好满足月度新增条件，添加四条月度新增记录防止月度新增服务多运行
                if (months == 12 || months == 120 || months == 240)
                {
                    List<AnnualLeaveTradeModel> trades = new List<AnnualLeaveTradeModel>();
                    trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = 0,Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 1,_state = "added",_id = "1"});
                    trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = 0,Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 2,_state = "added",_id = "2"});
                    trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = 0,Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 3,_state = "added",_id = "3"});
                    trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = 0,Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 4,_state = "added",_id = "4"});
                    await client.AddRecords<object>(annualLeaveTradeResid, trades);
                }
                return new object();
            }
            catch (Exception e)
            {
                WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// 给员工进行年初创建(员工年假季度账户表)
        /// </summary>
        /// <param name="employee">员工</param>
        /// <param name="year">年</param>
        /// <returns></returns>
        private async Task<object> CreateAccount(EmployeeModel employee, int year)
        {
            var njjdAccountModals = new List<NjjdAccountModal>();
            njjdAccountModals.Add(new NjjdAccountModal()
            {
                numberID = employee.jobId,
                memberID = employee.personId,
                snsy = 0,
                djfp = 0,
                sjsy = 0,
                year = year,
                quarter = 1,
                _state = "added",
                _id = "1"
            });
            njjdAccountModals.Add(new NjjdAccountModal()
            {
                numberID = employee.jobId,
                memberID = employee.personId,
                snsy = 0,
                djfp = 0,
                sjsy = 0,
                year = year,
                quarter = 2,
                _state = "added",
                _id = "1"
            });    
            njjdAccountModals.Add(new NjjdAccountModal()
            {
                numberID = employee.jobId,
                memberID = employee.personId,
                snsy = 0,
                djfp = 0,
                sjsy = 0,
                year = year,
                quarter = 3,
                _state = "added",
                _id = "1"
            });      
            njjdAccountModals.Add(new NjjdAccountModal()
            {
                numberID = employee.jobId,
                memberID = employee.personId,
                snsy = 0,
                djfp = 0,
                sjsy = 0,
                year = year,
                quarter = 1,
                _state = "added",
                _id = "1"
            });
            await client.AddRecords<object>(ygnjjdzhResid, njjdAccountModals);
            return new object();
        }
        
        /// <summary>
        /// 上年转入
        /// </summary>
        /// <param name="employee">员工</param>
        /// <returns></returns>
        /// 
        public  async Task<Object> TransferredLastYear(EmployeeModel employee, int year)
        {
            try
            {
                var option = new GetTableOptionsModal { };
                option.cmswhere = $"NumberID ={employee.jobId} and Quarter = '{year}'";
                var yearLeftRes = await client.getTable<YearLeftModel>(YearLeftResid, option);
                if (yearLeftRes.data.Count > 0)
                {
                    var annualLeaveTradeModel = new AnnualLeaveTradeModel()
                    {
                        NumberID = employee.jobId,
                        pnid = employee.personId,
                        Year = year,
                        Quarter = 1,
                        snsytrans = yearLeftRes.data[0].Residue,
                        djfptrans = 0,
                        sjsytrans = 0,
                        Type = "上年转入",
                        _state = "added",
                        _id = "0"
                    };
                    if (!await Utils.IsTradeExist("上年转入", year,1, employee.jobId))
                    {
                         await client.AddRecords<object>(annualLeaveTradeResid, new List<AnnualLeaveTradeModel> { annualLeaveTradeModel });
                    }
                }
                return new object();
            }
            catch (Exception e)
            {
                WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// 年初创建
        /// </summary>
        /// <param name="employee">员工</param>
        /// <returns></returns>
        /// 
        public  async Task<Object> BeginningOfYearCreate(EmployeeModel employee, int year)
        {
            try
            {
                var annualLeaveTradeModels = new List<AnnualLeaveTradeModel>(); //年假交易
                var annualLeaves = new double[] { }; //季度年假分配天数
                annualLeaves = GetQuarterAssignDays(GetTotalAnnualLeaveForNewEmployee(employee.serviceAge ?? 0));
                annualLeaveTradeModels.Add(
                    new AnnualLeaveTradeModel
                    {
                        NumberID = employee.jobId,
                        pnid = employee.personId,
                        Year = year,
                        Quarter = 1,
                        sjsytrans = 0,
                        snsytrans = 0,
                        djfptrans = annualLeaves[0],
                        Type = "年初创建",
                        _state = "added",
                        _id = "1"
                    });
                annualLeaveTradeModels.Add(
                    new AnnualLeaveTradeModel
                    {
                        NumberID = employee.jobId,
                        pnid = employee.personId,
                        Year = year,
                        Quarter = 2,
                        sjsytrans = 0,
                        snsytrans = 0,
                        djfptrans = annualLeaves[1],
                        Type = "年初创建",
                        _state = "added",
                        _id = "2"
                    });
                annualLeaveTradeModels.Add(
                    new AnnualLeaveTradeModel
                    {
                        NumberID = employee.jobId,
                        pnid = employee.personId,
                        Year = year,
                        Quarter = 3,
                        sjsytrans = 0,
                        snsytrans = 0,
                        djfptrans = annualLeaves[2],
                        Type = "年初创建",
                        _state = "added",
                        _id = "3"
                    });
                annualLeaveTradeModels.Add(
                    new AnnualLeaveTradeModel
                    {
                        NumberID = employee.jobId,
                        pnid = employee.personId,
                        Year = year,
                        Quarter = 4,
                        sjsytrans = 0,
                        snsytrans = 0,
                        djfptrans = annualLeaves[3],
                        Type = "年初创建",
                        _state = "added",
                        _id ="4"
                    });
                await client.AddRecords<object>(annualLeaveTradeResid, annualLeaveTradeModels);
                return new object();
            }
            catch (Exception e)
            {
                WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// 获得季度分配天数
        /// </summary>
        /// <param name="totalDays">总年假天数</param>
        /// <returns>0:1季度年假,1:2季度年假,2:3季度年假,3:4季度年假</returns>
        private double[] GetQuarterAssignDays(int totalDays)
        {
            double[] quarterDays = { 0, 0, 0, 0 };
            int startIndex = 0;
            float avg = (float)totalDays / (4 - startIndex);
            double leftDays = totalDays;
            while (startIndex < 4)
            {
                //平均数的整数位
                int integer = (int)avg;
                //平均数的小数位
                double _decimal = avg - (double)integer;
                if (_decimal > 0.5)
                {
                    integer++;
                    _decimal = 0;
                }
                else
                {
                    if (_decimal > 0)
                    {
                        _decimal = 0.5;
                    }
                }
                double day;
                if (leftDays > avg)
                {
                    day = integer + _decimal;
                }
                else
                {
                    day = leftDays;
                }
                leftDays = leftDays - day;
                quarterDays[startIndex] = day;
                startIndex++;
            }
            return quarterDays;
        }
        /// <summary>
        /// 获得新员工总年假天数
        /// </summary>
        /// <param name="workyears">新员工工龄</param>
        /// <returns></returns>
        private int GetTotalAnnualLeaveForNewEmployee(int serviceAge)
        {
            int days = 0;
            if (serviceAge < 10 && serviceAge >= 1)
            {
                days = 5;
            }
            if (serviceAge < 20 && serviceAge >= 10)
            {
                days = 10;
            }
            if (serviceAge >= 20)
            {
                days = 15;
            }
            return days;
        }
        /// <summary>
        /// 给员工分配年假
        /// <param name="employee">员工</param>
        /// </summary>
        public async Task<object> Distribution(EmployeeModel employee,int year,string date,int serviceMonths)
        {
            var ret = new { };
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            var quarter = GetQuarterByDate(date);
            var enterdate = Convert.ToDateTime(employee.enterDate);
            var increasedate= enterdate.AddMonths(serviceMonths).ToString(dateFormatString);
            var quarterDays= getQuarterTradsDays(quarter,increasedate);
            List<AnnualLeaveTradeModel> trades = new List<AnnualLeaveTradeModel>();
            if (1 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[0],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 1,_state = "added",_id = "1"});
            }
            if (2 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[1],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 2,_state = "added",_id = "2"});
            }
            if (3 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[2],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 3,_state = "added",_id = "3"});
            }
            if (4 >= quarter)
            {
                trades.Add(new AnnualLeaveTradeModel{pnid = employee.personId,snsytrans = 0,sjsytrans = 0,djfptrans = quarterDays[3],Type = "月度新增",NumberID = employee.jobId,Year = year,Quarter = 4,_state = "added",_id = "4"});
            }
            try
            {
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
        /// 获取各季度年假数
        /// <param name="currentQuarter">当前季度</param>
        /// <param name="date">日期</param>
        /// </summary>
        public double[] getQuarterTradsDays(int currentQuarter, string date)
        {
            double[] quarterDays = {0,0,0,0};
            int startIndex = currentQuarter - 1;
            //总可用天数
            int totalDays = getConversionDays(date);
            //WriteLine($"总年假天数:{totalDays}");

            double leftDays = totalDays;
            //每季度平均天数
            float avg =(float) totalDays / (4 - startIndex);
            //WriteLine($"平均每季度分配年假数{avg}");
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
                //WriteLine($"q{startIndex+1}分配的年假数:{day}------------剩余可分配年假数:{leftDays}");
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