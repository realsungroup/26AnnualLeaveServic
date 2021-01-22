using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonthlyNewlyIncreased.Constant;
using static System.Console;

namespace MonthlyNewlyIncreased.Tasks
{
    /// <summary>
    /// 员工年假使用剩余清零任务
    /// </summary>
    public class AnnualLeaveResidueResetTask
    {
        public AnnualLeaveResidueResetTask()
        {
            this.client = new LzRequest(realsunBaseURL);
            this.client.setHeaders(new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        /// <summary>
        /// 页码
        /// </summary>
        private string _pageNo = "0";
        /// <summary>
        /// 每页数量
        /// </summary>
        private string pageSize = "100";
        private LzRequest client = null;

        public async Task<dynamic> GetNjjdAccountList(int year, string pageNo, string[] numberIDs)
        {
            var option = new GetTableOptionsModal { };
            string sqlCondition = (numberIDs == null || numberIDs.Length == 0) ? string.Empty : $" and numberID in ({string.Join(',', numberIDs)})";
            option.pageSize = pageSize;
            option.pageIndex = pageNo;
            option.cmswhere = numberIDs == null ? $"Quarter=2 and  Year={year}{sqlCondition} ": $"Quarter=2 and  Year={year}{sqlCondition}";

            var rsp = await this.client.getTable<NjjdAccountModal>(ygnjjdzhResid, option);
            bool existNextPage = HasNextPage<NjjdAccountModal>(rsp, pageNo);
            return new { rsp.data, existNextPage };
        }
        /// <summary>
        /// 获取员工年假季度
        /// </summary>
        /// <param name="year"></param>
        /// <param name="numberID"></param>
        /// <returns></returns>
        public async Task<NjjdAccountModal> GetNjjdAccount(int year, string numberID)
        {
            var option = new GetTableOptionsModal { };
            option.pageSize = pageSize;
            option.cmswhere = $"Quarter=2 and  Year={year} and numberID={numberID}";

            var res = await this.client.getTable<NjjdAccountModal>(ygnjjdzhResid, option);
            if (res.data != null && res.data.Count > 0)
                return res.data[0];
            else
                return null;
        }
       
        public async Task<object> AddResidueReset(List<NjjdAccountModal> njjdAccountModals)
        {
            var ret = new { };
            var annualLeaveTradeModels = new List<AnnualLeaveTradeModel>();
            foreach (var item in njjdAccountModals)
            {
                annualLeaveTradeModels.Add(new AnnualLeaveTradeModel
                {
                    NumberID = item.numberID,
                    Name = item.name,
                    Type = "剩余清零",
                    Year = item.year,
                    Quarter = item.quarter,
                    snsytrans = item.snsy,
                    sjsytrans = item.sjsy,
                    djfptrans = item.djfp,
                    _state = "added"
                });
            }
            //await this.client.AddRecords<object>(annualLeaveTradeResid, annualLeaveTradeModels);
            return ret;
        }
        /// <summary>
        /// 批量处理剩余清零
        /// </summary>
        /// <param name="year"></param>
        /// <param name="numberIDs"></param>
        /// <param name="pageno"></param>
        /// <returns></returns>
        private async Task<object> BatchResidueReset(int year = 2021, string[] numberIDs = null, string pageno = "0")
        {
            var ret = new { };
            var rsp = await GetNjjdAccountList(year, pageno, numberIDs);
            if (rsp.data != null) AddResidueReset(rsp.data);
            if (rsp.existNextPage)
            {
                pageno = (Convert.ToInt16(pageno) + 1).ToString();
                await BatchResidueReset(year, numberIDs, pageno);
            }
            return ret;
        }
        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="year"></param>
        /// <param name="numberIDs"></param>
        /// <returns></returns>
        public async Task<object> Start(int year, params string[] numberIDs)
        {
            return await BatchResidueReset(year, numberIDs, _pageNo);
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
