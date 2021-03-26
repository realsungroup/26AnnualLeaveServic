using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using static System.Console;
using static MonthlyNewlyIncreased.Constant;
using MonthlyNewlyIncreased.Tasks;
using static MonthlyNewlyIncreased.Utils;

namespace MonthlyNewlyIncreased.Jobs {
    public class EntryAssignmentJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start (DateTime.Today);
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start (DateTime date) {
            var ret = new Hashtable ();
            var taskStartTime = DateTime.Now.ToString(datetimeFormatString);
            WriteLine($"开始执行入职分配-{taskStartTime}");
            var newEmployee = new NewEmployeeTask();
            var cmswhere = $"enterDate between '{date.AddDays(-30).ToString(dateFormatString)}' and '{date.ToString(dateFormatString)}'";
            await newEmployee.Run(cmswhere);
            WriteLine($"结束执行入职分配{DateTime.Now.ToString(datetimeFormatString)}");
            AddTask("入职分配",taskStartTime , DateTime.Now.ToString(datetimeFormatString), "");
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
            var jobDetail = JobBuilder.Create<EntryAssignmentJob> ().Build ();

            var trigger = TriggerBuilder.Create ()
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (0, 30))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}