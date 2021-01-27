using MonthlyNewlyIncreased.Tasks;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;
using static MonthlyNewlyIncreased.Utils;
using static MonthlyNewlyIncreased.Constant;

namespace MonthlyNewlyIncreased.Jobs
{
    /// <summary>
    /// 年假剩余清零
    /// </summary>
    public class MonthUseJob : IJob
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
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            WriteLine("开始执行月度使用：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var today = DateTime.Today;
            var monthUseTask = new MonthUseTask();
            monthUseTask.taskStartTime = taskStartTime;
            var quarter = GetQuarterByMonth(today.Month);
            await monthUseTask.Run(today.Year,quarter,today.ToString("MM"));
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
            WriteLine($"MonthUseJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<MonthUseJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder.CronSchedule("0 0 20 L * ?"))//每个月月末那天晚上8点
                .Build();

            // 添加调度
            return await scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}
