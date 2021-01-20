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
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;

namespace MonthlyNewlyIncreased.Jobs {
    public class MonthlyIncreasedJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var ret = new Hashtable ();
            WriteLine("开始执行月度结算");
            var today = DateTime.Today.ToString("MM-dd");
            var monthlyIncreased = new MonthlyIncreasedTask();
            await  monthlyIncreased.GetNewEmployeeList();
            foreach (var item in monthlyIncreased.employeeList)
            {
                var enterDate = item.enterDate.Substring(item.enterDate.Length - 5);
                //社龄是否增加
                if ((enterDate == today) && (item.serviceAge != null))
                {
                    int workingyears = (int)item.serviceAge + 1;
                    //增加后的社龄是否为1，10，20
                    if (workingyears==1||workingyears==10||workingyears==20)
                    {
                        await monthlyIncreased.Distribution(item);
                    }
                }
            }
            WriteLine($"结束执行月度结算{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
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
            WriteLine ($"EntryAssignmentJob init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<MonthlyIncreasedJob> ().Build ();

            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (0, 15))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}