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


namespace MonthlyNewlyIncreased.Jobs {
    public class EntryAssignmentJob : IJob {

        public async Task Execute (IJobExecutionContext context) {
            await start ();
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start () {
            var ret = new Hashtable ();
            WriteLine($"开始执行入职分配{DateTime.Now.ToString(datetimeFormatString)}");
            var newEmployee = new NewEmployeeTask();
            await  newEmployee.GetNewEmployeeList();
            foreach (var item in newEmployee.employeeList)
            {
                await newEmployee.Distribution(item);
            }
            WriteLine($"结束执行入职分配{DateTime.Now.ToString(datetimeFormatString)}");
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
                .WithSchedule (CronScheduleBuilder.DailyAtHourAndMinute (0, 15))
                .Build ();

            // 添加调度
            return await scheduler.ScheduleJob (jobDetail, trigger);
        }
    }
}