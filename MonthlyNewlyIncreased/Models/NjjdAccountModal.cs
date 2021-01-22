using System;

namespace MonthlyNewlyIncreased.Modals {
    /// <summary>
    /// 员工年假季度账户 Modal
    /// </summary>
    /// 
    public class NjjdAccountModal {
        //id
        public string REC_ID { get; set; }
        //工号
        public string numberID { get; set; }
        //姓名
        public string name { get; set; }
        //年度
        public int year { get; set; }
        //季度
        public int quarter { get; set; }
        //上年剩余余额
        public double snsy { get; set; }
        //当季分配余额
        public double djfp { get; set; }
        //上季剩余余额
        public double sjsy { get; set; }
        
        public string? isLock { get; set;}
        //
        public string? _state { get; set; }
        //
        public string? _id{ get; set; }
    }
}