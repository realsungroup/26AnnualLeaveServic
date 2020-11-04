using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;
using Flurl;
using static ShopAPI.Constant;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShopAPI.Http;

namespace ShopAPI.Jobs {
    /// <summary>
    /// 登录 realsun 平台的定时任务
    /// </summary>
    public class LoginRealsunJob : IJob {
        public async Task Execute (IJobExecutionContext context) {
            await start ();
            // return Task.CompletedTask;
        }
        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var client = new LzRequest (Constant.realsunBaseURL);
            var res = await client.login (Constant.realsunUsername, Constant.realsunPassword);
            var accessToken = (string) res.AccessToken;
            Constant.realsunAccessToken = accessToken;
            return res;
        }

        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> init () {
            var schedulerFactory = new StdSchedulerFactory ();
            var scheduler = await schedulerFactory.GetScheduler ();

            await scheduler.Start ();
            WriteLine ($"LoginRealsunJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<LoginRealsunJob> ().Build ();
            var trigger = TriggerBuilder.Create ()
                .WithSimpleSchedule (m => {
                    m.WithIntervalInHours (24).RepeatForever ();
                })
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}