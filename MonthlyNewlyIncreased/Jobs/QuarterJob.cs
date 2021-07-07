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
            WriteLine ($"QuarterJob start1");
            var ret = new Hashtable ();
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                var res =await client.getTable<QuarterConfigModel>(QuarterConfigResid);
                var today = DateTime.Today.ToString("MM-dd");
                WriteLine ($"QuarterJob start2");
                foreach (var config in res.data)
                {
                  
                    var date = Convert.ToDateTime(config.runDate).ToString("MM-dd");
                    WriteLine ($"QuarterJob start3："+today);
                    WriteLine ($"QuarterJob start4："+date);
                    if (date == today)
                    {
                        var year = DateTime.Today.Year;
                        if (config.quarter == 4)
                        {
                            var currentQuarter = GetQuarterByMonth(DateTime.Now.Month);
                            if (currentQuarter !=4)
                            {
                                year = year - 1;
                            }
                        }
                        WriteLine($"开始执行季度结算{DateTime.Now.ToString(datetimeFormatString)}");
                        var task = new QuarterTask();
                        task.taskStartTime = DateTime.Now.ToString(datetimeFormatString);
                        await task.Run(year, config.quarter);
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
            WriteLine ($"QuarterJob init 1622");
            // 创建作业和触发器
            var jobDetail = JobBuilder.Create<QuarterJob> ().Build ();
            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (16,51))
                .Build ();
            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}