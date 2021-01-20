using System;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 员工信息 Modal
    /// </summary>
    /// 
    public class EmployeeModel {
        //工号
        public string jobId { get; set; }
        //编号
        public string personId { get; set; }
        //姓名
        public string name { get; set; }

        //社会工龄
        public int? serviceAge { get; set; }
        //入职日期
        public string enterDate { get; set; }
    }
}