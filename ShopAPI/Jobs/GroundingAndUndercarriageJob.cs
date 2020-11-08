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
        public static async Task<object> start (bool debug = false) {
            var ret = new Hashtable ();

            var getGroundingAndUndercarriageGoodsListTask = new GetGroundingAndUndercarriageGoodsListTask ();
            var res = await getGroundingAndUndercarriageGoodsListTask.run ();

            var groundingGoodsList = new List<GroundingTableModal> ();
            var undercarriageGoodsList = new List<GroundingTableModal> ();

            foreach (var resItem in res) {
                var shopID = resItem.shopID;
                // 转换上架商品
                var gResult = DataCovertTask.goodsTalbe2GroundingTable (resItem.groundingGoodsList, "Y", shopID);
                groundingGoodsList.AddRange (gResult);
                // 转换下架商品
                var uResult = DataCovertTask.goodsTalbe2GroundingTable (resItem.undercarriageGoodsList, "N", shopID);
                undercarriageGoodsList.AddRange (uResult);
            }

            WriteLine ("开始上架、下架商品：");
            WriteLine ("上架商品数量：" + groundingGoodsList.Count);
            WriteLine ("下架商品数量：" + undercarriageGoodsList.Count);

            // 上架商品
            var gRes = await groundingGoods (groundingGoodsList);
            // 下架商品
            var uRes = await undercarriageGoods (undercarriageGoodsList);

            ret.Add ("res", res);
            ret.Add ("上架的商品", gRes);
            ret.Add ("下架的商品", uRes);

            return ret;
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

            var list = List2TwoDimensionList<GroundingTableModal> (goodsList);

            var ret = new List<object> ();
            var j = 1;
            foreach (var itemList in list) {
                WriteLine (j);
                WriteLine ("itemList.Count:" + itemList.Count);
                var res = await client.AddRecords<object> (groundingResid, goodsList);
                WriteLine ("end");
                j++;
                ret.Add (res);
            }

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

            var list = List2TwoDimensionList<GroundingTableModal> (goodsList);
            var ret = new List<object> ();
            var j = 1;
            foreach (var itemList in list) {
                WriteLine (j);
                WriteLine ("itemList.Count:" + itemList.Count);
                var res = await client.AddRecords<object> (groundingResid, goodsList);
                WriteLine ("end");
                j++;
                ret.Add (res);
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
            var jobDetail = JobBuilder.Create<SyncGoodsJob> ().Build ();
            var trigger = TriggerBuilder.Create ()
                .WithSimpleSchedule (m => {
                    m.WithIntervalInMinutes (30).RepeatForever ();
                })
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}