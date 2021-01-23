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
            foreach (var item in employeeModels)
            {
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 1));
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 2));
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 3));
                njjdAccountModals.Add(CreateNjjdAccountModal(item.jobId, item.name, year, 4));
            }
            //await this.client.AddRecords<object>(ygnjjdzhResid, njjdAccountModals);
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
        /// 年初创建+上年转入
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
                IntoYearLeft(rsp.data); //上年转入
            }
            pageNo = (Convert.ToInt16(pageNo) + 1).ToString();
            if (rsp.existNextPage) //存在下一页
                await CreatYearBeginningAndIntoYearLeft(year, employeeResid, pageNo);
            return new { };
        }
        private async Task<object> CreatYearBeginningAndIntoYearLeft(int year, string[] employeeResids, string pageNo = "0")
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
                        Type = "上年转入"
                    }
                    );
            }
            //await this.client.AddRecords<object>( annualLeaveTradeResid, annualLeaveTradeModels);
            return new { };
        }

        private NjjdAccountModal CreateNjjdAccountModal(string numberID, string name, int year, int quarter)
        {
            return new NjjdAccountModal()
            {
                numberID = numberID,
                name = name,
                year = year,
                quarter = quarter
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
