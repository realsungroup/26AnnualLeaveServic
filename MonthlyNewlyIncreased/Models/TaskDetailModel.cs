using System;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 任务详情 Modal
    /// </summary>
    /// 
    public class TaskDetailModel {
        //记录编号
        public string? recid  { get; set; }
        //任务名称
        public string task_name { get; set; }
        //工号
        public string work_id { get; set; }
        //开始时间
        public string start_time { get; set; }
        //结束时间
        public string end_time { get; set; }
        //错误信息
        public string error { get; set; }
        
        //
        public string? _state { get; set; }
        //
        public int? _id{ get; set; }
    }
}