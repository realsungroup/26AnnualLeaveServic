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
using static ShopAPI.Utils;

namespace ShopAPI.Jobs {
    /// <summary>
    /// 商品上架任务
    /// </summary>
    public class GroundingJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start (bool debug = false) {
            var ret = new Hashtable ();

            var task = new GetGroundingGoodsListTask ();
            var res = await task.run ();

            var goodsList = new List<GroundingTableModal> ();
            var undercarriageGoodsList = new List<GroundingTableModal> ();

            foreach (var resItem in res) {
                var shopID = resItem.shopID;
                // 转换上架商品
                var gResult = DataCovertTask.goodsTalbe2GroundingTable (resItem.goodsList, "Y", shopID);
                goodsList.AddRange (gResult);
            }

            WriteLine ("开始上架商品：");
            WriteLine ("上架商品数量：" + goodsList.Count);

            // 上架商品
            if (goodsList.Count != 0) {
                await groundingGoods (goodsList);
                // 等 1 毫秒后再上架商品
                System.Timers.Timer t = new System.Timers.Timer (1);
                t.Elapsed += new System.Timers.ElapsedEventHandler (timeout);
                t.AutoReset = false;
                t.Enabled = true;
            } else {
                // 等 10 分钟后再上架商品
                System.Timers.Timer t = new System.Timers.Timer (10 * 60 * 1000);
                t.Elapsed += new System.Timers.ElapsedEventHandler (timeout);
                t.AutoReset = false;
                t.Enabled = true;
            }

            return ret;
        }

        // 倒计时事件
        public static void timeout (object source, System.Timers.ElapsedEventArgs e) {
            // 继续上架商品
            start ();
        }

        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        public static async Task<object> groundingGoods (List<GroundingTableModal> goodsList) {
            WriteLine ("开始上架商品：");
            var client = new LzRequest (realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });

            var list = List2TwoDimensionList<GroundingTableModal> (goodsList, 20);

            WriteLine ("list.Count:" + list.Count);

            var ret = new List<object> ();
            var index = 1;
            foreach (var itemList in list) {
                WriteLine ($"{index} 正在上架的商品数量:" + itemList.Count);
                try {
                    await client.AddRecords<object> (groundingResid, itemList);
                } catch (System.Exception ex) {
                    WriteLine (ex.Message);
                }
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
            WriteLine ($"GroundingJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<GroundingJob> ().Build ();
            var trigger = TriggerBuilder.Create ()
                .WithIdentity ("myTrigger", "group1")
                .WithSimpleSchedule (m => {
                    // m.WithIntervalInMinutes (10);
                })
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}