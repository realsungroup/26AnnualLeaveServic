using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;
using Flurl;
using static ShopAPI.Constant;

namespace ShopAPI.Jobs {
    /// <summary>
    /// 登录 realsun 平台的定时任务
    /// </summary>
    public class LoginRealsunJob : IJob {
        public Task Execute (IJobExecutionContext context) {

            Console.WriteLine ("hhh");

            return Task.CompletedTask;
        }

        public static async Task<object> start () {
            var schedulerFactory = new StdSchedulerFactory ();
            var scheduler = await schedulerFactory.GetScheduler ();

            await scheduler.Start ();
            WriteLine ($"任务调度器已启动");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<SyncGoods> ().Build ();
            var trigger = TriggerBuilder.Create ()
                .WithSimpleSchedule (m => {
                    // m.WithIntervalInMinutes (10).RepeatForever ();
                    m.WithIntervalInSeconds (2).RepeatForever ();
                })
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}