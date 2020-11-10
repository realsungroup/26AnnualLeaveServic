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

namespace ShopAPI.Jobs {
    public class SyncGoodsJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start (string materialID = null, bool debug = false) {
            var ret = new Hashtable ();

            var task = new GetNeedSyncGoodsListTask (materialID, debug);
            var needSycnGoodsList = await task.run ();

            // 添加商品到 realsun 平台
            await addGoodsToRealsun (needSycnGoodsList);

            ret.Add ("需要同步的商品的数量", needSycnGoodsList.Count);

            return ret;
        }

        /// <summary>
        /// 添加商品到 realsun 平台
        /// </summary>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        public static async Task<object> addGoodsToRealsun (List<GoodsTableModal> goodsList) {
            var list = List2TwoDimensionList<GoodsTableModal> (goodsList, 20);

            var client = new LzRequest (realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });

            var ret = new List<object> ();
            var index = 1;
            foreach (var itemList in list) {
                WriteLine ($"{index} 同步商品数量:" + itemList.Count);
                try {
                    // 同步商品
                    await client.AddRecords<object> (goodsResid, itemList);
                    // 商品上架
                    await GroundingJob.start ();
                } catch (System.Exception) { }
                index++;
            }

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
            WriteLine ($"SyncGoodsJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<SyncGoodsJob> ().Build ();

            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (4, 0))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}