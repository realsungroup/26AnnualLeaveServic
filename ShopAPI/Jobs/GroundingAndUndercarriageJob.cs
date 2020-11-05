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
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopAPI.Jobs {
    /// <summary>
    /// 商品上下架任务
    /// </summary>
    public class GroundingAndUndercarriageJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start (string materialID = null, bool debug = false) {
            var ret = new Hashtable ();

            var getGroundingAndUndercarriageGoodsListTask = new GetGroundingAndUndercarriageGoodsListTask ();
            var res = await getGroundingAndUndercarriageGoodsListTask.run ();

            ret.Add ("res", res);

            return ret;
        }

        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> init () {
            var schedulerFactory = new StdSchedulerFactory ();
            var scheduler = await schedulerFactory.GetScheduler ();

            await scheduler.Start ();
            WriteLine ($"GroundingAndUndercarriageJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<SyncGoodsJob> ().Build ();
            var trigger = TriggerBuilder.Create ()
                .WithSimpleSchedule (m => {
                    m.WithIntervalInMinutes (10).RepeatForever ();
                })
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}