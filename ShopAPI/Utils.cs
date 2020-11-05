using System;

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
  }

}