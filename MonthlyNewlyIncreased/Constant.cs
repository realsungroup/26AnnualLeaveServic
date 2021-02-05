using Microsoft.Extensions.Configuration;
using System;

namespace MonthlyNewlyIncreased
{
    public class Constant
    {
        public static IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        /// <summary>
        /// 后端相关
        /// </summary>
        // 后端 api 基地址
        public static string realsunBaseURL = configuration["realsunBaseURL"];
        public static string WXBaseURL = configuration["WXBaseURL"];

        // 后端 api 登录用户名称
        public static string realsunUsername = configuration["realsunUsername"];

        // 后端 api 登录密码
        public static string realsunPassword = configuration["realsunPassword"];

        // 访问后端的 accessToken
        public static string realsunAccessToken = "";
        public static string dateFormatString = "yyyy-MM-dd";
        public static string datetimeFormatString = "yyyy-MM-dd HH:mm:ss";


        /// <summary>
        /// 表 resid
        /// </summary>

        //员工年假季度账户表
        public static string ygnjjdzhResid = "662169346288";

        //员工年假交易表
        public static string annualLeaveTradeResid = "662169358054";

        //21年后入职且在职的员工
        public static string newEmployeeResid = "664466340655";
        //20年及之前入职且在职的员工
        public static string oldEmployeeResid = "664466391252";

        //员工社保信息
        public static string SocialSecurityInfoResid = "662122466450";

        //考勤月报
        public static string MonthReportResid = "311025002785";

        //任务详情
        public static string TaskDetailResid = "664543217154";
        //任务
        public static string TaskResid = "664542578818";
        //上年剩余
        public static string YearLeftResid = "662169383744";

        //上年剩余
        public static string QuarterConfigResid = "664645272364";

        //个人银行卡表(微信端)
        public static string PersonalBankCardResid = "547032160625";
        //所有员工表
        public static string AllEmployeeResid = "227186227531";
        //员工银行卡表
        public static string EmployeeBankCardResid = "665841454598";

    }
}