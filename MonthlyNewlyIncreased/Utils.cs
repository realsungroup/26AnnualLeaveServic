using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonthlyNewlyIncreased.Models;
using MonthlyNewlyIncreased.Http;
using static System.Console;
using static MonthlyNewlyIncreased.Constant;

namespace MonthlyNewlyIncreased {

    class Utils {

        /// <summary>
        /// unix 时间戳转换为 DateTime
        /// </summary>
        /// <param name="unixTimeStamp">unix 时间戳（秒）</param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime (double unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds (unixTimeStamp).ToLocalTime ();
            return dtDateTime;
        }

        /// <summary>
        /// list 转换为二维 list
        /// </summary>
        /// <param name="list">list</param>
        /// <param name="count">二维数组元素的数量</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<List<T>> List2TwoDimensionList<T> (List<T> list, long count = 100) {
            var retList = new List<List<T>> ();
            var i = 1;
            var listIndex = 0;
            foreach (var item in list) {
                if (i == 1) {
                    var arr = new List<T> ();
                    arr.Add (item);
                    retList.Add (arr);
                    i++;
                } else if (i > 1 && i < count) {
                    retList[listIndex].Add (item);
                    i++;
                } else if (i == count) {
                    retList[listIndex].Add (item);
                    i = 1;
                    listIndex++;
                }
            }
            return retList;
        }

        public static int GetQuarterByDate(string date)
        {
            var month = Convert.ToDateTime(date).Month - 1;
            var quarter = month / 3 + 1;
            return quarter;
        }

        /// <summary>
        /// 判断交易是否已经产生
        /// </summary>
        /// <param name="type">类型 </param>
        /// <param name="year">年</param>
        /// <param name="quarter">季度</param>
        /// <param name="number">工号</param>
        /// <returns></returns>
        /// 
        public static async Task<bool> IsTradeExist(string type,int year,int quarter,string number)
        {
           var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                var result =
                    await client.getTable<AnnualLeaveTradeModel>(annualLeaveTradeResid,
                        new GetTableOptionsModal
                        {
                            cmswhere = $"Type = '{type}' and Year = '{year}' and Quarter = '{quarter}' and NumberID = '{number}'"
                        });
                bool isExist = result.data.Count > 0;
                return isExist;
            }
            catch (Exception e)
            {
                WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// 往后台增加一条任务详情记录
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="starTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="error">错误信息</param>
        /// <param name="number">工号</param>
        /// <returns></returns>
        public static async Task<object> AddTaskDetail(string name,string starTime,string endTime,string error,string number)
        {
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                List<TaskDetailModel> list = new List<TaskDetailModel>();
                list.Add(new TaskDetailModel{
                    task_name = name,
                    start_time = starTime,
                    end_time = endTime,
                    error = error,
                    work_id = number,
                    _id =1,
                    _state = "added"});
                await client.AddRecords<object>(TaskDetailResid,list);
                return new {};
            }
            catch (Exception e)
            {
                WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// 往后台增加一条任务记录
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="starTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="error">错误信息</param>
        /// <returns></returns>
        public static async Task<object> AddTask(string name,string starTime,string endTime,string error)
        {
            var client = new LzRequest(realsunBaseURL);
            client.setHeaders (new { Accept = "application/json", accessToken = realsunAccessToken });
            try
            {
                List<TaskModel> list = new List<TaskModel>();
                list.Add(new TaskModel{
                    task_name = name,
                    start_time = starTime,
                    end_time = endTime,
                    error = error,
                    _id =1,
                    _state = "added"});
                await client.AddRecords<object>(TaskResid,list);
                return new {};
            }
            catch (Exception e)
            {
                WriteLine(e);
                throw;
            }
        }
        
        public static int GetQuarterByMonth(int month)
        {
            int quarter = 1;
            if (month<=3)
            {
                quarter = 1;
            }  
            if (month >= 4 && month <= 6)
            {
                quarter = 2;
            }
            if (month >= 7 && month <= 9)
            {
                quarter = 3;
            }
            if (month >= 10 && month <= 12)
            {
                quarter = 4;
            }
            return quarter;
        }
    }

}