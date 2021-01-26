using System;
using System.Collections.Generic;

namespace MonthlyNewlyIncreased.Models {
    /// <summary>
    /// 修改员工信息 Modal
    /// </summary>
    /// 
    public class ResponseModel<T> {
        public List<T>? data  { get; set; }
        public int? Error { get; set; }
        public string message { get; set; }
    }
}