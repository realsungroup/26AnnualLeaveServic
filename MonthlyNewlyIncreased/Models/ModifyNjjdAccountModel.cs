using System;

namespace MonthlyNewlyIncreased.Modals {
    /// <summary>
    /// 修改用的员工年假季度账户 Modal
    /// </summary>
    /// 
    public class ModifyNjjdAccountModel {
        //id
        public string REC_ID { get; set; }
        
        public string locked { get; set;}
        public string SendBack { get; set;}
        //
        public string? _state { get; set; }
        //
        public string? _id{ get; set; }
    }
}