using System;
using System.Collections;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Tasks {
    public class SyncAnnualLeaveTask {
        public SyncAnnualLeaveTask()
        {
            client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            wxclient = new LzRequest(WXBaseURL);
            wxclient.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        private LzRequest client = null;
        private LzRequest wxclient = null;
        public static IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

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
        private bool HasNextPage (GetTagbleResponseModal<OutApplyModel> rsp) {
            if ((Convert.ToInt16(_pageNo )+1) *  Convert.ToInt16(pageSize) > Convert.ToInt16(rsp.total)) {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 获取记录
        /// </summary>
        public async Task<object> GetList () {
            var ret = new { };
            try
            {
                var quarter = GetQuarterByDate(DateTime.Today.ToString("yyyy-MM-dd"));
                var option = new GetTableOptionsModal{};
                option.pageSize = pageSize;
                option.pageIndex = _pageNo;
                option.cmswhere = configuration["cmswhereSql"] + $" and quarter={quarter}";
                var res = await wxclient.getTable<OutApplyModel>("663933483597",option);
                foreach (var item in res.data)
                {
                    OutApplyModel temp = new OutApplyModel();
                    temp = item;
                    await Sync(temp);
                }
                if (HasNextPage(res)) {
                    _pageNo =(Convert.ToInt16(_pageNo) + 1).ToString();
                    await GetList();
                }
            } catch (Exception exception) {
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// 同步年假季度微信端累计申请
        /// <param name="employee">员工</param>
        /// </summary>
        public async Task<object> Sync(OutApplyModel account)
        {
            var starttime = DateTime.Now.ToString(datetimeFormatString);
            var ret = new { };
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"numberID = '{account.numberID}' and year = '{account.year}' and quarter = '{account.quarter}'";
            try
            {
                var res = await client.getTable<InApplyModel>(ygnjjdzhResid,option);
                if (res.data.Count>0)
                {
                    var data = res.data[0];
                    List<InApplyModel> list = new List<InApplyModel>();
                    list.Add(new InApplyModel(){
                        year = data.year,
                        quarter = data.quarter,
                        numberID = data.numberID,
                        name = data.name,
                        C3_662492645279 = account.C3_662492645279,
                        freezeMobile = account.freezeMobile,
                        REC_ID = data.REC_ID,
                        _id =1,
                        _state = "modified"});
                    
                    var res1 = await client.AddRecords<Hashtable>(ygnjjdzhResid, list);
                    if (Convert.ToInt32(res1["error"]) == 0)
                    {
                        List<ModifyOutApplyModel> list1 = new List<ModifyOutApplyModel>();
                        list1.Add(new ModifyOutApplyModel(){
                            isNeedSyn = "",
                            REC_ID = account.REC_ID,
                            _id = 1,
                            _state = "modified"});
                        var result = await wxclient.AddRecords<object>("663933483597",list1);
                    }
                }
                else
                {
                    AddTaskDetail("同步年假季度微信端累计申请",
                        starttime,
                        DateTime.Now.ToString(datetimeFormatString),
                        "9091端口后台没有该员工",
                        account.numberID
                        );
                }
            }
            catch (Exception e)
            {
                WriteLine("同步年假季度微信端累计申请：" + e.Message);
                throw;
            }
            return ret;
        }
    }
    
    public class ModifyOutApplyModel
    {
        public string REC_ID  { get; set; }
        public string? isNeedSyn { get; set; }
        public string? _state { get; set; }
        public int? _id{ get; set; } 
    }
    public class OutApplyModel
    {
        public string REC_ID  { get; set; }
        public string numberID  { get; set; }
        public string name  { get; set; }
        public int year  { get; set; }
        public int quarter  { get; set; }
        public float C3_662492645279  { get; set; }
        public float freezeMobile  { get; set; }
    }
    public class InApplyModel
    {
        public string REC_ID  { get; set; }
        public string numberID  { get; set; }
        public string name  { get; set; }
        public int year  { get; set; }
        public int quarter  { get; set; }
        public Nullable<float>  C3_662492645279  { get; set; }
        public Nullable<float> freezeMobile  { get; set; }
        public string? _state { get; set; }
        public int? _id{ get; set; } 
    }
}