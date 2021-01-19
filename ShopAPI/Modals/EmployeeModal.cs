using System;

namespace ShopAPI.Modals {
    /// <summary>
    /// 员工信息 Modal
    /// </summary>
    /// 
    public class EmployeeModal {
        //工号
        public string jobId { get; set; }
        //编号
        public string personId { get; set; }
        //姓名
        public string name { get; set; }

        //社会工龄
        public string serviceAge { get; set; }
    }
}