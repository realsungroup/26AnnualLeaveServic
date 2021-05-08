using System;
using System.Collections;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Tasks {
    public class AccountAbnormalTask {

        public AccountAbnormalTask()
        {
            client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        private LzRequest client = null;

        /// <summary>
        /// 获取缺失的人员列表
        /// </summary>
        public async Task<object> GetNoEmployeeList () {
            var ret = new { };
            var option = new GetTableOptionsModal();
            option.cmswhere = "(year(C3_227193233656)>=2021)  and HRUSER_DEP1ID in('100','2000')  and C3_305737857578  not in (select  personId from CT663860903672)";
            try {
                var res = await this.client.getTable<Hashtable>("227186227531" ,option);
                var list = new List<AddEmployee>();
                var _id = 0;
                foreach (var item in res.data)
                {
                    list.Add(new AddEmployee{
                        personId = Convert.ToInt64(item["C3_305737857578"]),
                        jobId = Convert.ToString( item["C3_227192472953"]),
                        _id = _id++,
                        _state = "added"
                    });
                }
               await client.AddRecords<AddEmployee>(newEmployeeResid, list);
            } catch (Exception exception) {
                WriteLine($"error：{exception}");
                return ret;
            }
            return ret;
        }
        
        /// <summary>
        /// 获取缺失年假账户的人员列表
        /// </summary>
        public async Task<object> GetNoAccountList()
        {
            var ret = new { };
            var option = new GetTableOptionsModal { };
            option.cmswhere = "(year(C3_227193233656)=YEAR(getdate()) or C3_294355760203='Y') and (select count(*) from  CT662169346288 where memberID=C3_305737857578 )<4 and HRUSER_DEP1ID in('100','2000')";
            try
            {
                var res = await this.client.getTable<Hashtable>("227186227531", option);
                WriteLine(res.data.Count);
                foreach (var item in res.data)
                {
                }
            }
            catch (Exception exception)
            {
                WriteLine($"error：{exception}");
                return ret;
            }

            return ret;
        }
        
    }

    public class AddEmployee
    {
        public string jobId { get; set; }
        public Int64 personId { get; set; }
        //
        public string? _state { get; set; }
        public int? _id{ get; set; }
    }
}