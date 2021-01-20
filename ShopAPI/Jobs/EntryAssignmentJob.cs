using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;
using ShopAPI.Http;
using static ShopAPI.Constant;
using System.Collections.Generic;
using ShopAPI.Modals;
using ShopAPI.Tasks;
using static ShopAPI.Utils;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopAPI.Jobs {
    public class EntryAssignmentJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var ret = new Hashtable ();
            WriteLine("开始执行入职分配");
            var newEmployee = new NewEmployee();
            await  newEmployee.GetNewEmployeeList();
            foreach (var item in newEmployee.employeeList)
            {
                await newEmployee.Distribution(item.jobId);
            }
            WriteLine($"结束执行入职分配{DateTime.Now.ToString("yyyy-MM-Dd HH:mm:ss")}");
            // ret.Add ("需要同步的商品的数量", needSycnGoodsList.Count);
            return ret;
        }
        
        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> init () {
            var schedulerFactory = new StdSchedulerFactory ();
            var scheduler = await schedulerFactory.GetScheduler ();

            await scheduler.Start ();
            WriteLine ($"EntryAssignmentJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<EntryAssignmentJob> ().Build ();

            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (0, 15))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}