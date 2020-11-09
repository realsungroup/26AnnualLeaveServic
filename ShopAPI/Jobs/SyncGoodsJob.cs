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

            // if (debug) {
            //     var _1_inValid_佣金比例 = task._1_inValid_佣金比例;
            //     var _2_inValid_优惠券数量 = task._2_inValid_优惠券数量;
            //     var _3_inValid_优惠券结束时间 = task._3_inValid_优惠券结束时间;
            //     var _4_inValid_是否品牌精选 = task._4_inValid_是否品牌精选;
            //     var _5_inValid_价格 = task._5_inValid_价格;
            //     var _6_inValid_优惠券金额 = task._6_inValid_优惠券金额;

            //     ret.Add ("_1_inValid_佣金比例", new { total = _1_inValid_佣金比例.Count, list = _1_inValid_佣金比例, fieldName = "CommissionRate" });
            //     ret.Add ("needSycnGoodsList", new { total = needSycnGoodsList.Count, list = needSycnGoodsList });
            //     ret.Add ("_2_inValid_优惠券数量", new { total = _2_inValid_优惠券数量.Count, list = _2_inValid_优惠券数量, fieldName = "CouponRemainCount" });
            //     ret.Add ("_3_inValid_优惠券结束时间", new { total = _3_inValid_优惠券结束时间.Count, list = _3_inValid_优惠券结束时间, fieldName = "CouponEndTime" });
            //     ret.Add ("_4_inValid_是否品牌精选", new { total = _4_inValid_是否品牌精选.Count, list = _4_inValid_是否品牌精选, fieldName = "SuperiorBrand" });
            //     ret.Add ("_5_inValid_价格", new { total = _5_inValid_价格.Count, list = _5_inValid_价格, fieldName = "ZkFinalPrice" });
            //     ret.Add ("_6_inValid_优惠券金额", new { total = _6_inValid_优惠券金额.Count, list = _6_inValid_优惠券金额, fieldName = "CouponAmount" });
            // }

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
            var j = 1;
            foreach (var itemList in list) {
                WriteLine (j);
                WriteLine ("itemList.Count:" + itemList.Count);
                // 同步商品
                var res = await client.AddRecords<object> (goodsResid, itemList);
                // 商品上架
                await GroundingJob.start ();
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