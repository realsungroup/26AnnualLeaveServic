using System;

namespace MonthlyNewlyIncreased
{
    public class Constant
    {
        /// <summary>
        /// 后端相关
        /// </summary>
        // 后端 api 基地址
        public static string realsunBaseURL = "http://10.108.2.66:9091/";
        public static string WXBaseURL = "http://kingofdinner.realsun.me:9091/";
        
        // 后端 api 登录用户名称
        public static string realsunUsername = "demo";

        // 后端 api 登录密码
        public static string realsunPassword = "1234@qwer";

        // 访问后端的 accessToken
        public static string realsunAccessToken = "";
        public static string dateFormatString = "yyyy-MM-dd";
        
        /// <summary>
        /// 表 resid
        /// </summary>
        
        //员工年假季度账户表
        public static string ygnjjdzhResid = "662169346288";
        //员工年假季度账户表
        public static string annualLeaveTradeResid = "662169358054";
        
        //21年后入职的员工
        public static string newEmployeeResid = "663860903672";
        
    }
}