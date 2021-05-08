using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;
using MonthlyNewlyIncreased.Http;
using static MonthlyNewlyIncreased.Constant;
using System.Collections.Generic;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Tasks;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Jobs {
    public class AccountAbnormalJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var ret = new Hashtable ();
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            WriteLine($"开始年假账户异常处理{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            var task = new AccountAbnormalTask();
            await  task.GetNoEmployeeList();
            AddTask("年假账户异常处理",taskStartTime , DateTime.Now.ToString(datetimeFormatString), "");
            WriteLine($"结束年假账户异常处理{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
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
            WriteLine ($"年假账户异常处理 init");

            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<AccountAbnormalJob> ().Build ();

            var trigger = TriggerBuilder.Create().StartNow().
                WithSchedule(SimpleScheduleBuilder.Create().WithIntervalInMinutes(10).RepeatForever())
                .Build ();;

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}