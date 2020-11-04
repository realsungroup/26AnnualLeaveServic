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
    public class SyncGoodsJob : IJob {
        /// <summary>
        /// 物料ID表记录 Modal
        /// </summary>
        public class MaterialRecordModal {
            public string material_name { get; set; }
            public string business_ID { get; set; }
            public string material_ID { get; set; }
            public string is_valid { get; set; }
            public string is_selection { get; set; }
        }
        /// <summary>
        /// 商户设置表 Modal
        /// </summary>
        public class commercialTenantSetModal {
            public string business_ID { get; set; }
            public string shop_ID { get; set; }
            public string commission_rate { get; set; }
            public string coupon_remain_count { get; set; }
            public string coupon_end_day { get; set; }
            public string superior_brand { get; set; }
            public string sale_price_up { get; set; }
            public string sale_price_down { get; set; }
        }

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<Hashtable> start (bool debug = false) {
            var ret = new Hashtable ();

            var getNeedSyncGoodsListTask = new GetNeedSyncGoodsListTask ();
            var needSycnGoodsList = await getNeedSyncGoodsListTask.run ();
            ret.Add ("needSycnGoodsList", needSycnGoodsList);

            if (debug) {
                var inValid_价格 = getNeedSyncGoodsListTask.inValid_价格;
                var inValid_优惠券数量 = getNeedSyncGoodsListTask.inValid_优惠券数量;
                var inValid_优惠券结束时间 = getNeedSyncGoodsListTask.inValid_优惠券结束时间;
                var inValid_优惠券金额 = getNeedSyncGoodsListTask.inValid_优惠券金额;
                var inValid_佣金比例 = getNeedSyncGoodsListTask.inValid_佣金比例;
                var inValid_是否品牌精选 = getNeedSyncGoodsListTask.inValid_是否品牌精选;

                ret.Add ("needSycnGoodsList", new { total = needSycnGoodsList.Count, list = needSycnGoodsList });

                ret.Add ("inValid_价格", new { total = inValid_价格.Count, list = inValid_价格, fieldName = "ZkFinalPrice" });

                ret.Add ("inValid_优惠券数量", new { total = inValid_优惠券数量.Count, list = inValid_优惠券数量, fieldName = "CouponRemainCount" });

                ret.Add ("inValid_优惠券结束时间", new { total = inValid_优惠券结束时间.Count, list = inValid_优惠券结束时间, fieldName = "CouponEndTime" });

                ret.Add ("inValid_优惠券金额", new { total = inValid_优惠券金额.Count, list = inValid_优惠券金额, fieldName = "CouponAmount" });

                ret.Add ("inValid_佣金比例", new { total = inValid_佣金比例.Count, list = inValid_佣金比例, fieldName = "CommissionRate" });

                ret.Add ("inValid_是否品牌精选", new { total = inValid_是否品牌精选.Count, list = inValid_是否品牌精选, fieldName = "SuperiorBrand" });
            }

            // var willSyncGoodsList = DataCovert.taobaoGoodsList2realsunGoodsList (needSycnGoodsList);

            // ret.Add ("willSyncGoodsList", willSyncGoodsList);

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
                .WithSimpleSchedule (m => {
                    m.WithIntervalInMinutes (10).RepeatForever ();
                })
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}