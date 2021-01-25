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
            if (DateTime.Today.Day > 28)
            {
            }
            else
            {
                await start ();
            }
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            WriteLine($"开始执行月度结算{DateTime.Now.ToString(datetimeFormatString)}");
            var monthlyIncreased = new MonthlyIncreasedTask();
            var year = DateTime.Today.Year;
            var date = DateTime.Today.ToString(dateFormatString);
            await monthlyIncreased.Run(DateTime.Today.ToString("dd"),year,date);
            AddTask("月度新增",taskStartTime , DateTime.Now.ToString(datetimeFormatString), "");
            WriteLine($"结束执行月度结算{DateTime.Now.ToString(datetimeFormatString)}");
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
            WriteLine ($"EntryAssignmentJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<MonthlyIncreasedJob> ().Build ();

            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (0, 15))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}