using System;

namespace ShopAPI.Modals {
    /// <summary>
    /// 员工年假季度账户 Modal
    /// </summary>
    /// 
    public class AnnualLeaveTradeModal {
        //工号
        public string NumberID { get; set; }
        //类型
        public string Type { get; set; }
        //年度
        public int Year { get; set; }
        //季度
        public int Quarter { get; set; }
        //上年剩余交易
        public float snsytrans { get; set; }
        //上季剩余交易
        public float sjsytrans { get; set; }
        //当季分配交易
        public float djfptrans { get; set; }

        //
        public string? _state { get; set; }
        //
        public string? _id{ get; set; }
    }
}