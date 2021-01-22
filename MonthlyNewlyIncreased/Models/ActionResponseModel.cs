using System;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 员工信息 Modal
    /// </summary>
    /// 
    public class ActionResponseModel {
        //错误码
        public int error  { get; set; }
        //错误信息
        public string message { get; set; }
    }
}