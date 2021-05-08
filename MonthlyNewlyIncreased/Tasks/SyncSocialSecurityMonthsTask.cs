using System;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using MonthlyNewlyIncreased.Models;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Tasks {
    public class SyncSocialSecurityMonthsTask {

        public SyncSocialSecurityMonthsTask()
        {
            client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            wxclient = new LzRequest(WXBaseURL);
            wxclient.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
        }
        private LzRequest client = null;
        private LzRequest wxclient = null;

        

        /// <summary>
        /// 获取到的员工
        /// </summary>
        public List<EmployeeModel> employeeList = new List<EmployeeModel> ();

        /// <summary>
        /// 获取新员工
        /// </summary>
        public async Task<object> GetNewEmployeeList () {
            var ret = new { };
            var option = new GetTableOptionsModal{};
            //只查询社保月数为null的数据
            option.cmswhere = $"wxMonths=0";
            try {
                var res = await this.client.getTable<EmployeeModel>(newEmployeeResid,option);
                foreach (var item in res.data)
                {
                    await SyncMonths(item);
                }
            } catch (System.Exception exception) {
                Console.WriteLine($"error：{exception}");
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// 同步社保信息
        /// <param name="employee">员工</param>
        /// </summary>
        public async Task<object> SyncMonths(EmployeeModel employee)
        {
            var starttime = DateTime.Now.ToString(datetimeFormatString);
            var ret = new { };
            var option = new GetTableOptionsModal{};
            option.cmswhere = $"C3_664296546211 = '{employee.jobId}'";
            try
            {
                var res = await wxclient.getTable<SocialSecurityInfoModel>(SocialSecurityInfoResid,option);
                if (res.data.Count>0)
                {
                    var data = res.data[0];
                    List<EmployeeModel> list = new List<EmployeeModel>();
                    list.Add(new EmployeeModel{
                        enterDate = employee.enterDate,
                        REC_ID = employee.REC_ID,
                        jobId = employee.jobId,
                        wxMonths = data.C3_662122615028,
                        _id =1,
                        _state = "modified"});
                    await client.AddRecords<object>(newEmployeeResid, list);
                }
                else
                {
                    WriteLine($"同步社保信息出错：微信后台社保信息表中没有员工[{employee.name}-{employee.jobId}]");
                    AddTaskDetail("同步社保信息",
                        starttime,
                        DateTime.Now.ToString(datetimeFormatString),
                        "微信后台没有该员工",
                        employee.jobId
                        );
                }
            }
            catch (Exception e)
            {
                WriteLine("同步社保信息出错：",e.Message);
                throw;
            }
            return ret;
        }
        
    }
}