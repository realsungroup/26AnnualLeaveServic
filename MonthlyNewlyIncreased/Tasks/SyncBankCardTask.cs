using System;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;

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
        public async Task<dynamic> GetEmployeeList(string pageNo)
        {
            var option = new GetTableOptionsModal { };
            option.pageSize = pageSize;
            option.pageIndex = pageNo;

            var rsp = await this.client.getTable<AllEmployeeModel>(AllEmployeeResid, option);
            bool existNextPage = HasNextPage<AllEmployeeModel>(rsp, pageNo);
            return new { rsp.data, existNextPage };
        }
        /// <summary>
        /// 通过工号ID获得员工
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public async Task<AllEmployeeModel> GetEmployee(string jobID)
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
        /// 同步一个人的银行卡号
        /// </summary>
        /// <param name="employeeModel"></param>
        /// <returns></returns>
        private async Task<ActionResponseModel> SyncBankCard(AllEmployeeModel allEmployeeModel)
        {
            var option = new GetTableOptionsModal { };
            var starttime = DateTime.Now.ToString(datetimeFormatString);
            option.cmswhere = $"C3_594120941744={allEmployeeModel.C3_227192472953}";

            try
            {
                var rsp = await this.wxclient.getTable<PersonalBankCardModel>(PersonalBankCardResid, option);
                if (rsp.data != null && rsp.data.Count > 0)
                {
                    allEmployeeModel.C3_497724880304 = rsp.data[0].C3_547032175037;
                    allEmployeeModel.C3_497724865718 = rsp.data[0].C3_547144218383;
                    allEmployeeModel._state = "modified";
                    allEmployeeModel._id = 1;
                    await client.AddRecords<object>(AllEmployeeResid, new List<AllEmployeeModel> { allEmployeeModel });
                }
                else
                {
                    Console.WriteLine($"微信端未找到工号：{allEmployeeModel.C3_227192472953}的员工");
                    AddTaskDetail("同步银行卡信息", starttime, DateTime.Now.ToString(datetimeFormatString), "微信后台没有该员工", allEmployeeModel.C3_227192472953);
                    return new ActionResponseModel() { error = -1, message = $"微信端未找到工号：{allEmployeeModel.C3_227192472953}的员工" };
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
        /// 同步所有员工的银行卡信息
        /// </summary>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public async Task<object> SyncBankCards(string pageNo = "0")
        {
            var rsp = await GetEmployeeList(pageNo); //获得员工+是否存在下页标识
            var index = 1;
            if (rsp.data != null)
            {
                foreach (AllEmployeeModel item in rsp.data)
                {
                    await SyncBankCard(item.C3_227192472953);
                    //Console.WriteLine($"工号：{item.C3_227192472953}同步完银行卡信息---{index}"); index++;
                }
            }
            pageNo = (Convert.ToInt32(pageNo) + 1).ToString();
            if (rsp.existNextPage) //存在下一页
                await SyncBankCards(pageNo);
            return new { };
        }
        /// <summary>
        /// 根据工号同步银行卡信息
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public async Task<ActionResponseModel> SyncBankCard(string jobID)
        {
            var starttime = DateTime.Now.ToString(datetimeFormatString);
            var allEmployeeModel = await GetEmployee(jobID);
            if (allEmployeeModel != null)
            {
                return await SyncBankCard(allEmployeeModel);
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
