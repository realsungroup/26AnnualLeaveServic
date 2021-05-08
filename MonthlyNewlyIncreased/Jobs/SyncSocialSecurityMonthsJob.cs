using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;
using MonthlyNewlyIncreased.Http;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Jobs {
    public class SyncSocialSecurityMonthsJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var ret = new Hashtable ();
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            WriteLine($"开始执行社保同步{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            var monthlyIncreasedTask = new SyncSocialSecurityMonthsTask();
            await  monthlyIncreasedTask.GetNewEmployeeList();
            AddTask("同步社保月数",taskStartTime , DateTime.Now.ToString(datetimeFormatString), "");
            WriteLine($"结束执行社保同步{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
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
            WriteLine ($"社保同步 init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<SyncSocialSecurityMonthsJob> ().Build ();

            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (0, 15))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}