using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;

namespace ShopAPI.Jobs {
    public class SyncGoods : IJob {
        public Task Execute (IJobExecutionContext context) {
            // WriteLine ("开始执行任务");
            // var updatedMin = DateTime.Now.AddMinutes (-10).ToString ("yyyy-MM-ddTHH:mm:ss.000Z");
            // var updatedMax = DateTime.Now.ToString ("yyyy-MM-ddTHH:mm:ss.000Z");

            // var username = "REALSUN_INTEGRATION";
            // var password = "Welcome26";

            // var task = new UpdateRealsunPersonTask (username, password, updatedMin, updatedMax);
            // task.start ();

            Console.WriteLine ("hhh");

            return Task.CompletedTask;
        }
        // public static async Task<object> start () {
        // var schedulerFactory = new StdSchedulerFactory ();
        // var scheduler = await schedulerFactory.GetScheduler ();

        // await scheduler.Start ();
        // WriteLine ($"任务调度器已启动");

        // // 创建作业和触发器
        // var jobDetail = JobBuilder.Create<SyncGoods> ().Build ();
        // var trigger = TriggerBuilder.Create ()
        //     .WithSimpleSchedule (m => {
        //         m.WithIntervalInMinutes (10).RepeatForever ();
        //     })
        //     .Build ();

        // // 添加调度
        // return await scheduler.ScheduleJob (jobDetail, trigger);
        // }
    }
}