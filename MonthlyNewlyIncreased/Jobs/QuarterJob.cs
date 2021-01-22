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
using Newtonsoft.Json;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Jobs {
    public class QuarterJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var ret = new Hashtable ();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                var res =await client.getTable<QuarterConfigModel>(QuarterConfigResid);
                var today = DateTime.Today.ToString("MM-dd");
                foreach (var config in res.data)
                {
                    var date = Convert.ToDateTime(config.runDate).ToString("MM-dd");
                    if (date == today)
                    {
                        WriteLine(config.quarter);
                        var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
                        WriteLine($"开始执行季度结算{DateTime.Now.ToString(datetimeFormatString)}");
                        var task = new QuarterTask();
                        task.taskStartTime = DateTime.Now.ToString(datetimeFormatString);
                        await task.Run(DateTime.Today.Year, config.quarter);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine(e);
                throw;
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
            WriteLine ($"QuarterJob init");
            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<QuarterJob> ().Build ();
            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (16, 18))
                .Build ();
            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}