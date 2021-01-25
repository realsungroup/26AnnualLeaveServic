using MonthlyNewlyIncreased.Http;
using MonthlyNewlyIncreased.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MonthlyNewlyIncreased.Constant;

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
        public async Task<dynamic> GetEmployeeList(string pageNo, string resid)
        {
            var option = new GetTableOptionsModal { };
            option.pageSize = pageSize;
            option.pageIndex = pageNo;

            var rsp = await this.client.getTable<EmployeeModel>(resid, option);
            bool existNextPage = HasNextPage<EmployeeModel>(rsp, pageNo);
            return new { rsp.data, existNextPage };
        }
        /// <summary>
        /// 给员工进行年初创建(员工年假季度账户表)
        /// </summary>
        /// <param name="pageNo">查询页</param>
        /// <param name="resid">员工表id</param>
        /// <returns></returns>
        private async Task<object> CreateYearBeginning(List<EmployeeModel> employeeModels, int year)
        {
            var njjdAccountModals = new List<NjjdAccountModal>();
            int _id = 0;
            foreach (var item in employeeModels)
            {
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 1, _id)); _id++;
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 2, _id)); _id++;
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 3, _id)); _id++;
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 4, _id)); _id++;
            }
            await this.client.AddRecords<object>(ygnjjdzhResid, njjdAccountModals);
            return new { };
        }
        
        /// <summary>
        ///  给表中员工进行年初创建(员工年假季度账户表)
        /// </summary>
        /// <param name="pageNo">在查询结果中,给设置页及以后所有页员工进行年初创建</param>
        /// <param name="year">年初创建的年份</param>
        /// <param name="employeeResid">被查询的员工表</param>
        /// <returns></returns>
        private async Task<object> CreateYearBeginning_All(int year, string employeeResid, string pageNo = "0")
        {
            var rsp = await GetEmployeeList(pageNo, employeeResid);
            if (rsp.data != null) CreateYearBeginning(rsp.data, year);

            pageNo = (Convert.ToInt16(pageNo) + 1).ToString();
            if (rsp.existNextPage) //存在下一页
                await CreateYearBeginning_All(year, employeeResid, pageNo);
            return new { };
        }

        /// <summary>
        /// 年初创建+季度分配+上年转入
        /// </summary>
        /// <param name="year"></param>
        /// <param name="employeeResid"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        private async Task<object> CreatYearBeginningAndIntoYearLeft(int year, string employeeResid, string pageNo = "0")
        {
            var rsp = await GetEmployeeList(pageNo, employeeResid); //获得员工+是否存在下页标识
            if (rsp.data != null)
            {
                CreateYearBeginning(rsp.data, year);  //年初创建               
                await DoQuarterAssignForEmployee(rsp.data, year, employeeResid == newEmployeeResid);  //季度分配               
                IntoYearLeft(rsp.data); //上年转入
            }
            pageNo = (Convert.ToInt16(pageNo) + 1).ToString();
            if (rsp.existNextPage) //存在下一页
                await CreatYearBeginningAndIntoYearLeft(year, employeeResid, pageNo);
            return new { };
        }
        /// <summary>
        /// 执行员工的季度分配
        /// </summary>
        /// <param name="employeeModels"></param>
        /// <param name="year"></param>
        /// <param name="isNewEmplopee">是否为新员工</param>
        /// <returns></returns>
        private async Task<object> DoQuarterAssignForEmployee(List<EmployeeModel> employeeModels, int year, bool isNewEmplopee)
        {
            var annualLeaveTradeModels = new List<AnnualLeaveTradeModel>(); //年假交易
            int _id = 0;

            foreach (var item in employeeModels)
            {
                var annualLeaves = new double[] { }; //季度年假分配天数
                if (isNewEmplopee) //新员工季度分配天数
                    annualLeaves = GetQuarterAssignDays(GetTotalAnnualLeaveForNewEmployee(item.serviceAge ?? 0));
                else //老员工季度分配天数
                    annualLeaves = GetQuarterAssignDays(GetTotalAnnualLeaveForOldEmployee(Convert.ToInt16(item.totalHolidays)));
                annualLeaveTradeModels.Add(
                   new AnnualLeaveTradeModel
                   {
                       NumberID = item.jobId,
                       Name = item.name,
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
                       NumberID = item.jobId,
                       Name = item.name,
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
                       NumberID = item.jobId,
                       Name = item.name,
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
                       NumberID = item.jobId,
                       Name = item.name,
                       Year = year,
                       Quarter = 4,
                       djfptrans = annualLeaves[3],
                       Type = "年初创建",
                       _state = "added",
                       _id = _id.ToString()
                   });
                _id++;
            }
            await this.client.AddRecords<object>(annualLeaveTradeResid, annualLeaveTradeModels);
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
        private int GetTotalAnnualLeaveForNewEmployee(int workyears)
        {
            int days = 0;
            if (workyears < 10 && workyears >= 1)
            {
                days = 5;
            }
            if (workyears < 20 && workyears >= 10)
            {
                days = 10;
            }
            if (workyears >= 20)
            {
                days = 15;
            }
            return days;
        }
        /// <summary>
        /// 获得老员工年假总天数
        /// </summary>
        /// <param name="totalHolidays">老员工上年年假天数</param>
        /// <returns></returns>
        private int GetTotalAnnualLeaveForOldEmployee(int totalHolidays)
        {
            return ++totalHolidays;
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
            string[] employeeResids = { newEmployeeResid };
            await CreatYearBeginningAndIntoYearLeft(year, employeeResids);
            return new { };
        }
        /// <summary>
        /// 上年转入
        /// </summary>
        /// <param name="employeeModels"></param>
        /// <returns></returns>
        private async Task<object> IntoYearLeft(List<EmployeeModel> employeeModels)
        {
            var yearLeftModels = GetYearLeft(employeeModels);
            if (yearLeftModels != null)
                await InsertForAnnualLeaveTrade(yearLeftModels.Result);
            return new { };
        }
        /// <summary>
        /// 通过员工工号获取年假上年剩余明细
        /// </summary>
        /// <param name="employeeModels">被查询的员工</param>
        /// <returns></returns>
        private async Task<List<YearLeftModel>> GetYearLeft(List<EmployeeModel> employeeModels)
        {
            var option = new GetTableOptionsModal { };
            var jobIds = employeeModels.Select(e => e.jobId).ToArray();
            var sqlCondition = string.Join(',', jobIds);
            option.cmswhere = $"NumberID in ({sqlCondition})";
            var rsp = await this.client.getTable<YearLeftModel>(YearLeftResid, option);
            return rsp.data;
        }
        /// <summary>
        /// 向年假使用明细(交易)中插入
        /// </summary>
        /// <param name="yearLeftModels"></param>
        /// <returns></returns>
        private async Task<object> InsertForAnnualLeaveTrade(List<YearLeftModel> yearLeftModels)
        {
            var annualLeaveTradeModels = new List<AnnualLeaveTradeModel>();
            int _id = 0;
            foreach (var item in yearLeftModels)
            {
                annualLeaveTradeModels.Add(
                    new AnnualLeaveTradeModel()
                    {
                        NumberID = item.NumberID,
                        Name = item.Name,
                        Year = item.Quarter,
                        Quarter = item.C3_663098635076,
                        snsytrans = item.Residue,
                        Type = "上年转入",
                        _state = "added",
                        _id = _id.ToString()
                    }
                    );
                _id++;
            }
        await this.client.AddRecords<object>( annualLeaveTradeResid, annualLeaveTradeModels);
            return new { };
        }

        private NjjdAccountModal CreateNjjdAccountModal(string numberID, string name, int year, int quarter, int _id)
        {
            return new NjjdAccountModal()
            {
                numberID = numberID,
                name = name,
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
