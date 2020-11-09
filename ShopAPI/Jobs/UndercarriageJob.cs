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
    /// 商品下架任务
    /// </summary>
    public class UndercarriageJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var ret = new Hashtable ();

            var task = new GetUndercarriageGoodsListTask ();
            var res = await task.run ();

            var goodsList = new List<GroundingTableModal> ();

            foreach (var resItem in res) {
                var shopID = resItem.shopID;
                // 转换下架商品
                var result = DataCovertTask.goodsTalbe2GroundingTable (resItem.goodsList, "N", shopID);
                goodsList.AddRange (result);
            }

            WriteLine ("开始下架商品：");
            WriteLine ("下架商品数量：" + goodsList.Count);

            // 下架商品
            await undercarriageGoods (goodsList);

            ret.Add ("下架的商品数量：", goodsList.Count);

            return ret;
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        public static async Task<object> undercarriageGoods (List<GroundingTableModal> goodsList) {
            WriteLine ("开始下架商品：");

            var client = new LzRequest (realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });

            var list = List2TwoDimensionList<GroundingTableModal> (goodsList, 20);
            var ret = new List<object> ();
            var j = 1;
            foreach (var itemList in list) {
                WriteLine (j);
                WriteLine ("itemList.Count:" + itemList.Count);
                var res = await client.AddRecords<object> (groundingResid, itemList);
                WriteLine ("end");
                j++;
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
            WriteLine ($"GroundingAndUndercarriageJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<UndercarriageJob> ().Build ();
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