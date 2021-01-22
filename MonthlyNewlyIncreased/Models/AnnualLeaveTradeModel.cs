using System;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 员工年假季度账户 Modal
    /// </summary>
    /// 
    public class AnnualLeaveTradeModel {
        //工号
        public string NumberID { get; set; }
        //姓名
        public string Name { get; set; }
        //类型
        public string Type { get; set; }
        //年度
        public int Year { get; set; }
        //季度
        public int Quarter { get; set; }
        //上年剩余交易
        public double snsytrans { get; set; }
        //上季剩余交易
        public double sjsytrans { get; set; }
        //当季分配交易
        public double djfptrans { get; set; }

        //
        public string? _state { get; set; }
        //
        public string? _id{ get; set; }
    }
}