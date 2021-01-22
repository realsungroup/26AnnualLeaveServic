using System;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 员工社保信息 Model
    /// </summary>
    /// 
    public class MonthReportModel {
        //工号
        public string YGNO { get; set; }
        
        //年假数
        public double F_23 { get; set; }
    }
}