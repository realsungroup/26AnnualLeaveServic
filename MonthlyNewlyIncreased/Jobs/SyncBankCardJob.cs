using MonthlyNewlyIncreased.Tasks;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MonthlyNewlyIncreased.Constant;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Jobs
{
    public class SyncBankCardJob : IJob
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
            Console.WriteLine($"开始执行银行卡信息同步{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            var syncBankCardTask = new SyncBankCardTask();
            await syncBankCardTask.SyncBankCards();
            AddTask("同步银行卡信息", taskStartTime, DateTime.Now.ToString(datetimeFormatString), "");
            Console.WriteLine($"结束执行银行卡信息同步{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
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
            Console.WriteLine($"银行卡信息同步 init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<SyncBankCardJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
                .Build();

            // 添加调度
            return await scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}
