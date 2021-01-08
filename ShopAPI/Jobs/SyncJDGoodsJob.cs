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

namespace ShopAPI.Jobs
{
    /// <summary>
    /// 同步京东商品定时任务
    /// </summary>
    public class SyncJDGoodsJob : IJob
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
            var task = new SyncJDGoodsListTask();
            await task.run();
            return new { };
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
            WriteLine($"SyncGoodsJob3 init");

            await start();
            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<SyncJDGoodsJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(4, 0))
                .Build();

            // 添加调度
            return await scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}