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
    /// 年初创建和上年转入任务
    /// </summary>
    public class CreateYearBeginningAndIntoLastYearJob : IJob
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
            if (DateTime.Now.Month == 1) //每年1月1日执行
            {
                var startTime = DateTime.Now.ToString(datetimeFormatString);
                WriteLine("开始执行年初创建和上年转入");
                var today = DateTime.Today.ToString("MM-dd");
                var creatYearBeginningAndIntoYearLeft = new CreatYearBeginningAndIntoYearLeftTask();
                await creatYearBeginningAndIntoYearLeft.Start(DateTime.Now.Year);
                WriteLine($"结束执行年初创建和上年转入{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                await AddTask("年初创建和上年转入", startTime, DateTime.Now.ToString(datetimeFormatString), "");
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

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<CreateYearBeginningAndIntoLastYearJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(3, 0, 0))
                .Build();

            // 添加调度
            return await scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}
