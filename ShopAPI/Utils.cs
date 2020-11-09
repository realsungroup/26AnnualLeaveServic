using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;
using static ShopAPI.Constant;

namespace ShopAPI {

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

        // public static async Task<object> LoopAddRecords<AddRecordsDel> (AddRecordsDel callback, string resid, List<List<object>> data) {
        //     var ret = new List<object> ();
        //     var j = 1;
        //     foreach (var itemList in data) {
        //         WriteLine (j);
        //         WriteLine ("itemList.Count:" + itemList.Count);
        //         var res = callback<object> (goodsResid, itemList);
        //         WriteLine ("end");
        //         j++;
        //         ret.Add (res);
        //     }
        // }
    }

}