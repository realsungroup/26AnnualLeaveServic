using System;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 修改员工信息 Modal
    /// </summary>
    /// 
    public class ModifyEmployeeModel {
        //记录编号
        public string REC_ID  { get; set; }
        //社保月数
        public int? totalMonth { get; set; }
        //
        public string? _state { get; set; }
        //
        public int? _id{ get; set; }
    }
}