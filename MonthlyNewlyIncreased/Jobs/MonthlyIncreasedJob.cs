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
    public class MonthlyIncreasedJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start(DateTime.Today);
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start (DateTime DTdate) {
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            WriteLine($"开始执行月度新增-{taskStartTime}");
            var monthlyIncreased = new MonthlyIncreasedTask();
            var year = DTdate.Year;
            var date = DTdate.ToString(dateFormatString);
            await monthlyIncreased.Run(year,date);
            AddTask("月度新增",taskStartTime , DateTime.Now.ToString(datetimeFormatString), "");
            WriteLine($"结束执行月度新增{DateTime.Now.ToString(datetimeFormatString)}");
            return new {};
        } 
        
        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> init () {
            var schedulerFactory = new StdSchedulerFactory ();
            var scheduler = await schedulerFactory.GetScheduler ();

            await scheduler.Start ();
            WriteLine ($"MonthlyIncreasedJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<MonthlyIncreasedJob> ().Build ();

            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (6, 0))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}