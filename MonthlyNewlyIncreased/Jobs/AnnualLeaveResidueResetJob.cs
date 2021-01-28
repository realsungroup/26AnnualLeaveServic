using MonthlyNewlyIncreased.Tasks;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;
using static MonthlyNewlyIncreased.Constant;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Jobs
{
    /// <summary>
    /// 年假剩余清零
    /// </summary>
    public class AnnualLeaveResidueResetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await start();
        }
        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start()
        {
            var ret = new Hashtable();
            if (DateTime.Now.Month == 7) //每年七月执行
            {
                var startTime = DateTime.Now.ToString(datetimeFormatString);
                WriteLine("开始执行剩余清零");
                var today = DateTime.Today.ToString("MM-dd");
                var annualLeaveResidueReset = new AnnualLeaveResidueResetTask();
                await annualLeaveResidueReset.Start(DateTime.Now.Year);
                WriteLine($"结束执行剩余清零{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                await AddTask("剩余清零", startTime, DateTime.Now.ToString(datetimeFormatString), "");
            }
            return ret;
        }

        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> init()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();

            await scheduler.Start();
            WriteLine($"AnnualLeaveResidueResetJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<AnnualLeaveResidueResetJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(1, 0, 0))
                .Build();

            // 添加调度
            return await scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}
