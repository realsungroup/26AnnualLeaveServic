using MonthlyNewlyIncreased.Http;
using MonthlyNewlyIncreased.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MonthlyNewlyIncreased.Constant;
using static MonthlyNewlyIncreased.Utils;
using static System.Console;

namespace MonthlyNewlyIncreased.Tasks
{
    /// <summary>
    /// 年初创建和上年转入
    /// </summary>
    public class CreatYearBeginningAndIntoYearLeftTask
    {
        public CreatYearBeginningAndIntoYearLeftTask()
        {
            this.client = new LzRequest(realsunBaseURL);
            this.client.setHeaders(new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        /// <summary>
        /// 每页数量
        /// </summary>
        private string pageSize = "100";
        private LzRequest client = null;
        /// <summary>
        /// 获取员工
        /// </summary>
        /// <param name="pageNo">查询页</param>
        /// <param name="resid">员工表id</param>
        /// <returns></returns>
        public async Task<dynamic> GetEmployeeList( string resid)
        {

            var rsp = await this.client.getTable<EmployeeModel>(resid);
            return new { rsp.data };
        }
        public async Task<dynamic> GetEmployee(string numberID)
        {
            var option = new GetTableOptionsModal { };
            option.pageSize = pageSize;
            option.cmswhere = $"jobId ={numberID}";

            var rsp = await this.client.getTable<EmployeeModel>(oldEmployeeResid, option);
            if (rsp.data != null && rsp.data.Count > 0) //查询老员工
            {
                return new { Employee = rsp.data[0], IsNewEmployee = false };
            }
            else //查询新员工
            {
                var newRsp = await this.client.getTable<EmployeeModel>(newEmployeeResid, option);
                if (newRsp.data != null && newRsp.data.Count > 0)
                {
                    return new { Employee = newRsp.data[0], IsNewEmployee = true };
                }
            }

            return new { Employee = string.Empty, IsNewEmployee = true };
        }
        /// <summary>
        /// 给员工进行年初创建(员工年假季度账户表)
        /// </summary>
        /// <param name="pageNo">查询页</param>
        /// <param name="resid">员工表id</param>
        /// <returns></returns>
        private async Task<object> CreateYearBeginning(EmployeeModel employeeModel, int year)
        {
            var njjdAccountModals = new List<NjjdAccountModal>();
            string startTime = DateTime.Now.ToString(datetimeFormatString);
            njjdAccountModals.Add(CreateNjjdAccountModal(employeeModel.jobId, year, 1, 1));
            njjdAccountModals.Add(CreateNjjdAccountModal(employeeModel.jobId, year, 2, 2));
            njjdAccountModals.Add(CreateNjjdAccountModal(employeeModel.jobId, year, 3, 3));
            njjdAccountModals.Add(CreateNjjdAccountModal(employeeModel.jobId, year, 4, 4));
            await this.client.AddRecords<object>(ygnjjdzhResid, njjdAccountModals);
            return new { };
        }

        /// <summary>
        /// 年假季度账户表中是否已存在记录
        /// </summary>
        /// <param name="year"></param>
        /// <param name="quarter"></param>
        /// <param name="numberID"></param>
        /// <returns></returns>
        private async Task<bool> IsExistInYgnjjdzh(int year, int quarter, string numberID)
        {
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders(new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                var result =
                    await client.getTable<AnnualLeaveTradeModel>(ygnjjdzhResid,
                        new GetTableOptionsModal
                        {
                            cmswhere = $"Year = '{year}' and Quarter = '{quarter}' and NumberID = '{numberID}'"
                        });
                return result.data != null && result.data.Count > 0;
            }
            catch (Exception)
            {
                await AddTaskDetail("年初创建", DateTime.Now.ToString(datetimeFormatString), DateTime.Now.ToString(datetimeFormatString), $"{year}年{quarter}季度{numberID}工号的员工在季度账户表中查询失败", numberID);
                return false;
            }
        }
        /// <summary>
        /// 年初创建+季度分配+上年转入
        /// </summary>
        /// <param name="year"></param>
        /// <param name="employeeResid"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        private async Task<object> CreatYearBeginningAndIntoYearLeft(int year, string employeeResid)
        {
            var rsp = await GetEmployeeList(employeeResid); //获得员工
            foreach (EmployeeModel item in rsp.data)
            {
                await CreateYearBeginning(item, year);  //年初创建               
                await DoQuarterAssignForEmployee(item, year, false);  //季度分配               
                await IntoYearLeft(item,year); //上年转入
            }
            return new { };
        }
        private async Task<object> CreateYearBeginningAndIntoYearLeftForNumberID(int year, string numberID)
        {
            var rsp = await GetEmployee(numberID);
            var startTime = DateTime.Now.ToString(datetimeFormatString);

            if (!(rsp.Employee is string))
            {
                await CreateYearBeginning(rsp.Employee, year);  //年初创建               
                await DoQuarterAssignForEmployee(rsp.Employee, year, rsp.IsNewEmployee);  //季度分配               
                await IntoYearLeft(rsp.Employee,year); //上年转入                
            }
            else
            {
                await AddTaskDetail("年初创建", startTime, DateTime.Now.ToString(datetimeFormatString), $"{year}年{numberID}工号的员工不存在", numberID);
            }
            return new { };
        }
        /// <summary>
        /// 执行员工的季度分配
        /// </summary>
        /// <param name="employeeModels"></param>
        /// <param name="year"></param>
        /// <param name="isNewEmployee">是否为新员工</param>
        /// <returns></returns>
        private async Task<object> DoQuarterAssignForEmployee(EmployeeModel employeeModel, int year, bool isNewEmployee)
        {
            var annualLeaveTradeModels = new List<AnnualLeaveTradeModel>(); //年假交易
            int _id = 0;
            string startTime = DateTime.Now.ToString(datetimeFormatString);
            var annualLeaves = new double[] { }; //季度年假分配天数
            if (isNewEmployee) //新员工季度分配天数
                annualLeaves = GetQuarterAssignDays(GetTotalAnnualLeaveForNewEmployee(employeeModel.serviceAge ?? 0));
            else //老员工季度分配天数
                annualLeaves = GetQuarterAssignDays(GetTotalAnnualLeaveForOldEmployee(employeeModel.serviceAge ?? 0, employeeModel.enterDate));
            annualLeaveTradeModels.Add(
               new AnnualLeaveTradeModel
               {
                   NumberID = employeeModel.jobId,
                   Name = employeeModel.name,
                   Year = year,
                   Quarter = 1,
                   djfptrans = annualLeaves[0],
                   Type = "年初创建",
                   _state = "added",
                   _id = _id.ToString()
               });
            _id++;
            annualLeaveTradeModels.Add(
               new AnnualLeaveTradeModel
               {
                   NumberID = employeeModel.jobId,
                   Name = employeeModel.name,
                   Year = year,
                   Quarter = 2,
                   djfptrans = annualLeaves[1],
                   Type = "年初创建",
                   _state = "added",
                   _id = _id.ToString()
               });
            _id++;
            annualLeaveTradeModels.Add(
               new AnnualLeaveTradeModel
               {
                   NumberID = employeeModel.jobId,
                   Name = employeeModel.name,
                   Year = year,
                   Quarter = 3,
                   djfptrans = annualLeaves[2],
                   Type = "年初创建",
                   _state = "added",
                   _id = _id.ToString()
               });
            _id++;
            annualLeaveTradeModels.Add(
               new AnnualLeaveTradeModel
               {
                   NumberID = employeeModel.jobId,
                   Name = employeeModel.name,
                   Year = year,
                   Quarter = 4,
                   djfptrans = annualLeaves[3],
                   Type = "年初创建",
                   _state = "added",
                   _id = _id.ToString()
               });
            _id++;
            foreach (var item in annualLeaveTradeModels)
            {
                var isExist = await IsTradeExist("年初创建", year, item.Quarter ?? 1, employeeModel.jobId);
                if (!isExist)
                {
                    var rsp = await this.client.AddRecords<object>(annualLeaveTradeResid, new List<AnnualLeaveTradeModel>() { item });
                    var JRsp = (JObject)rsp;
                    if (JRsp["Error"].ToObject<int>() != 0)
                    {
                        await AddTaskDetail("年初创建(季度分配)", startTime, DateTime.Now.ToString(datetimeFormatString), JRsp["message"].ToString(), employeeModel.jobId);
                    }
                }
                else //已存在季度分配
                {
                    await AddTaskDetail("年初创建(季度分配)", startTime, DateTime.Now.ToString(datetimeFormatString), $"{year}年{item.Quarter}季度{employeeModel.jobId}工号的员工已季度分配过", employeeModel.jobId);
                }
            }
            return new { };
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
        /// 获得老员工年假总天数
        /// </summary>
        /// <param name="serviceAge">社会工龄</param>
        /// <param name="enterDate">入职日期</param>
        /// <returns></returns>
        private int GetTotalAnnualLeaveForOldEmployee(int serviceAge, string enterDate)
        {
            var enter = DateTime.Parse(enterDate);
            var workYears = 2022 - enter.Year; //相差年份数
            WriteLine($"workyear：{workYears}");
            if (workYears == 0) //入职还没跨年
            {
                workYears = 0; //第一年
            }
            else
            {
                workYears = enter.Month >= 7 ? workYears - 1 : workYears; //第几年是从0开始的，所以要减1。当年7月份及以后入职的，到下一年的1月1日仍算第1年反之算第2年
            }

            int totalDays = 0;
            if (serviceAge < 10)
            {
                switch (workYears)
                {
                    case 0: //第一年
                        totalDays = 8;
                        break;
                    case 1:
                        totalDays = 9;
                        break;
                    case 2:
                        totalDays = 10;
                        break;
                    case 3:
                        totalDays = 11;
                        break;
                    case 4:
                        totalDays = 12;
                        break;
                    case 5:
                        totalDays = 13;
                        break;
                    case 6:
                        totalDays = 14;
                        break;
                    default:
                        totalDays = 15;
                        break;
                }
            }
            else if (serviceAge < 20)
            {
                switch (workYears)
                {
                    case 0:
                    case 1:
                    case 2:
                        totalDays = 10;
                        break;
                    case 3:
                        totalDays = 11;
                        break;
                    case 4:
                        totalDays = 12;
                        break;
                    case 5:
                        totalDays = 13;
                        break;
                    case 6:
                        totalDays = 14;
                        break;
                    default:
                        totalDays = 15;
                        break;
                }
            }
            else
            {
                totalDays = 15;
            }
            return totalDays;
        }
        private async Task<object> CreatYearBeginningAndIntoYearLeft(int year, string[] employeeResids)
        {
            foreach (var item in employeeResids)
            {
                await CreatYearBeginningAndIntoYearLeft(year, item);
            }
            return new { };
        }
        public async Task<object> Start(int year)
        {
            string[] employeeResids = { oldEmployeeResid };
            await CreatYearBeginningAndIntoYearLeft(year, employeeResids);
            return new { };
        }
        /// <summary>
        /// 给指定员工进行年初创建+上年转入
        /// </summary>
        /// <param name="year"></param>
        /// <param name="numberIDs"></param>
        /// <returns></returns>
        public async Task<object> Start(int year, params string[] numberIDs)
        {
            foreach (var item in numberIDs)
            {
                await CreateYearBeginningAndIntoYearLeftForNumberID(year, item);
            }
            return new { };
        }
        /// <summary>
        /// 上年转入
        /// </summary>
        /// <param name="employeeModels"></param>
        /// <returns></returns>
        private async Task<object> IntoYearLeft(EmployeeModel employeeModel, int year)
        {
            var yearLeftModels = await GetYearLeft(employeeModel,year);
            if (yearLeftModels != null && yearLeftModels.Count > 0)
                await InsertForAnnualLeaveTrade(yearLeftModels[0]);
            return new { };
        }
        /// <summary>
        /// 通过员工工号获取年假上年剩余明细
        /// </summary>
        /// <param name="employeeModels">被查询的员工</param>
        /// <returns></returns>
        private async Task<List<YearLeftModel>> GetYearLeft(EmployeeModel employeeModel,int year)
        {
            var option = new GetTableOptionsModal { };
            option.cmswhere = $"NumberID ={employeeModel.jobId} and Quarter = '{year}'";
            var rsp = await this.client.getTable<YearLeftModel>(YearLeftResid, option);
            return rsp.data;
        }
        /// <summary>
        /// 向年假使用明细(交易)中插入
        /// </summary>
        /// <param name="yearLeftModels"></param>
        /// <returns></returns>
        private async Task<object> InsertForAnnualLeaveTrade(YearLeftModel yearLeftModel)
        {
            int _id = 0;
            var startTime = DateTime.Now.ToString(datetimeFormatString);

            var annualLeaveTradeModel = new AnnualLeaveTradeModel()
            {
                NumberID = yearLeftModel.NumberID,
                Name = yearLeftModel.Name,
                Year = yearLeftModel.Quarter,
                Quarter = yearLeftModel.C3_663098635076,
                snsytrans = yearLeftModel.Residue,
                Type = "上年转入",
                _state = "added",
                _id = _id.ToString()
            };
         
            if (!await IsTradeExist("上年转入", annualLeaveTradeModel.Year, annualLeaveTradeModel.Quarter ?? 1, annualLeaveTradeModel.NumberID))
            {
                var rsp = await this.client.AddRecords<object>(annualLeaveTradeResid, new List<AnnualLeaveTradeModel> { annualLeaveTradeModel });
                var JRsp = (JObject)rsp;
                if (JRsp["Error"].ToObject<int>() != 0)
                {
                    await AddTaskDetail("上年转入", startTime, DateTime.Now.ToString(datetimeFormatString), JRsp["message"].ToString(), annualLeaveTradeModel.NumberID);
                }
            }
            else //上年转入已存在
            {             
                await AddTaskDetail("上年转入", startTime, DateTime.Now.ToString(datetimeFormatString), $"{annualLeaveTradeModel.Year}年{annualLeaveTradeModel.Quarter}季度{annualLeaveTradeModel.NumberID}工号的上年转入已存在", annualLeaveTradeModel.NumberID);
            }            
            return new { };
        }

        private NjjdAccountModal CreateNjjdAccountModal(string numberID, int year, int quarter, int _id)
        {
            return new NjjdAccountModal()
            {
                numberID = numberID,
                year = year,
                quarter = quarter,
                _state = "added",
                _id = _id.ToString()
            };
        }
        /// <summary>
        /// 是否还有下一页数据
        /// </summary>
        /// <param name="rsp"></param>
        /// <returns></returns>
        private bool HasNextPage<T>(GetTagbleResponseModal<T> rsp, string pageNo)
        {
            if ((Convert.ToInt16(pageNo) + 1) * Convert.ToInt16(this.pageSize) > Convert.ToInt16(rsp.total))
            {
                return false;
            }
            return true;
        }
    }
}
