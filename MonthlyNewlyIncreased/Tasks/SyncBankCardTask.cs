using System;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;
using System.Diagnostics;

namespace MonthlyNewlyIncreased.Tasks
{
    /// <summary>
    /// 同步员工银行卡信息(外网->内网)
    /// </summary>
    public class SyncBankCardTask
    {
        public SyncBankCardTask()
        {
            client = new LzRequest(realsunBaseURL);
            client.setHeaders(new { Accept = "application/json", accessToken = realsunAccessToken });
            wxclient = new LzRequest(WXBaseURL);
            wxclient.setHeaders(new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        private LzRequest client = null;
        private LzRequest wxclient = null;
        /// <summary>
        /// 每页数量
        /// </summary>
        private string pageSize = "100";
        /// <summary>
        /// 获取员工
        /// </summary>
        /// <param name="pageNo">查询页</param>
        /// <param name="resid">员工表id</param>
        /// <returns></returns>
        private async Task<dynamic> GetEmployeeList(string pageNo)
        {
            var option = new GetTableOptionsModal { };
            option.pageSize = pageSize;
            option.pageIndex = pageNo;
            option.cmswhere = "C3_294355760203='Y'";  //仅查询在职的

            var rsp = await this.client.getTable<AllEmployeeModel>(AllEmployeeResid, option);
            bool existNextPage = HasNextPage<AllEmployeeModel>(rsp, pageNo);
            return new { rsp.data, existNextPage };
        }
        /// <summary>
        /// 通过工号ID获得员工
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        private async Task<AllEmployeeModel> GetEmployee(string jobID)
        {
            var option = new GetTableOptionsModal { };
            option.cmswhere = $"C3_227192472953={jobID}";
            var rsp = await this.client.getTable<AllEmployeeModel>(AllEmployeeResid, option);
            if (rsp.data != null && rsp.data.Count > 0)
            {
                return rsp.data[0];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 是否还有下一页数据
        /// </summary>
        /// <param name="rsp"></param>
        /// <returns></returns>
        private bool HasNextPage<T>(GetTagbleResponseModal<T> rsp, string pageNo)
        {
            if ((Convert.ToInt32(pageNo) + 1) * Convert.ToInt16(this.pageSize) > Convert.ToInt32(rsp.total))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 通过员工工号同步银行卡信息
        /// </summary>
        /// <param name="employeeModel"></param>
        /// <returns></returns>
        private async Task<ActionResponseModel> SyncBankCardByJobID(string jobID)
        {
            var option = new GetTableOptionsModal { };
            var starttime = DateTime.Now.ToString(datetimeFormatString);
            var employeeBankCardModel = new EmployeeBankCardModel();
            option.cmswhere = $"C3_594120941744={jobID}";

            try
            {
                var rsp = await this.wxclient.getTable<PersonalBankCardModel>(PersonalBankCardResid, option); //微信端查询到的员工银行卡信息
                if (rsp.data != null && rsp.data.Count > 0)
                {
                    employeeBankCardModel.C3_675883566967 = rsp.data[0].C3_594120941744;//工号
                    employeeBankCardModel.C3_675883596703 = rsp.data[0].C3_547144218383;//开户行信息
                    employeeBankCardModel.C3_675883584577 = rsp.data[0].C3_547032175037; //银行卡号
                    employeeBankCardModel._id = "1";
                    var existingEmployeeBankCard = await GetEmployeeBankCard(employeeBankCardModel.C3_675883566967); //员工银行卡表中是否存在记录
                    if (existingEmployeeBankCard == null) //员工银行卡表,还未存在记录
                    {
                        employeeBankCardModel._state = "added";
                    }
                    else//员工银行卡表,已存在记录
                    {
                        employeeBankCardModel.REC_ID = existingEmployeeBankCard.REC_ID;
                        employeeBankCardModel._state = "modified";
                    }
                    await client.AddRecords<object>(EmployeeBankCardResid, new List<EmployeeBankCardModel> { employeeBankCardModel });
                }
                else
                {
                    Console.WriteLine($"微信端未找到工号：{jobID}的员工");
                    AddTaskDetail("同步银行卡信息", starttime, DateTime.Now.ToString(datetimeFormatString), "微信后台没有该员工", jobID);
                    return new ActionResponseModel() { error = -1, message = $"微信端未找到工号：{jobID}的员工" };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"同步银行卡信息出错：{e.Message}");
                throw new Exception($"同步银行卡信息出错：{e.Message}");
            }
            return new ActionResponseModel() { error = 0, message = "已同步银行卡信息" };
        }
        /// <summary>
        /// 获得员工银行卡
        /// </summary>
        /// <param name="jobID">员工工号</param>
        /// <returns></returns>
        private async Task<EmployeeBankCardModel> GetEmployeeBankCard(string jobID)
        {
            var employeeBankCardModel = new EmployeeBankCardModel();
            var option = new GetTableOptionsModal { };
            option.cmswhere = $"C3_675883566967={jobID}";
            var rsp = await this.client.getTable<EmployeeBankCardModel>(EmployeeBankCardResid, option);
            if (rsp.data != null && rsp.data.Count > 0)
            {
                return rsp.data[0];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 同步所有员工的银行卡信息
        /// </summary>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public async Task<object> SyncBankCards(string pageNo = "0")
        {
            var rsp = await GetEmployeeList(pageNo); //获得员工+是否存在下页标识            
            if (rsp.data != null)
            {
                foreach (AllEmployeeModel item in rsp.data)
                {
                    await SyncBankCardByJobID(item.C3_227192472953);                   
                }
            }
            pageNo = (Convert.ToInt32(pageNo) + 1).ToString();
            if (rsp.existNextPage) //存在下一页
                await SyncBankCards(pageNo);
            return new { };
        }
        /// <summary>
        /// 同步银行卡信息
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public async Task<ActionResponseModel> SyncBankCard(string jobID)
        {
            var starttime = DateTime.Now.ToString(datetimeFormatString);
            var allEmployeeModel = await GetEmployee(jobID);
            if (allEmployeeModel != null) //检查全部员工表中是否有该员工
            {
                return await SyncBankCardByJobID(jobID);
            }
            else
            {
                await AddTaskDetail("同步银行卡信息", starttime, DateTime.Now.ToString(datetimeFormatString), "后台没有该员工", jobID);
                Console.WriteLine($"同步银行卡信息失败，后台未找到工号：{jobID}的员工");
                return new ActionResponseModel() { error = -1, message = $"同步银行卡信息失败，后台未找到工号：{jobID}的员工" };
            }
        }
    }
}
