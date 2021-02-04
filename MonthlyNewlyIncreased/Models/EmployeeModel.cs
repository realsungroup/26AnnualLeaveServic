using System;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 员工信息 Modal
    /// </summary>
    /// 
    public class EmployeeModel {
        //记录编号
        public string? REC_ID  { get; set; }
        //工号
        public string jobId { get; set; }
        //入职日期
        public string enterDate { get; set; }
        //入职天
        public string dayNum { get; set; }
        //社会工龄
        public int? serviceAge { get; set; }
        //社保月数
        public int? totalMonth { get; set; }
        //
        public string? _state { get; set; }
        //
        public int? _id{ get; set; }
        //姓名
        public string name { get; set; }
        //分配假期总数(老员工)
        public string totalHolidays { get; set; }      
        
    }
}