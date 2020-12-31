using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using static System.Console;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace ShopAPI.Http
{
    public class JDClient
    {
        public JDClient(DateTime dateTime, string appKey, string appSecret, string method, string accessToken,
            string buyParamJson360, string version = "1.0")
        {
            this.dateTime = dateTime;
            this.appKey = appKey;
            this.appSecret = appSecret;
            this.method = method;
            this.accessToken = accessToken;
            this.buyParamJson360 = buyParamJson360;
            this.version = version;
        }

        public DateTime dateTime { set; get; }
        public string appKey { set; get; }
        public string appSecret { set; get; }
        public string method { set; get; }
        public string accessToken { set; get; }
        public string buyParamJson360 { set; get; }
        public string version { set; get; }


        public async Task<T> execute<T>()
        {
            var url = getUrl();
            var res = await url
                .GetJsonAsync<T>();
            return res;
        }


        public string getUrl()
        {
            var timestamp = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + dateTime.ToString("zzzz")
                .Replace
                    (":", "");
            var sign = getSign(appSecret, buyParamJson360, accessToken, appKey, method, timestamp, version);
            var v = version;
            var url =
                $"https://api.jd.com/routerjson?sign={sign}&timestamp={HttpUtility.UrlEncode(timestamp)}&v={v}&app_key={appKey}&method={method}&access_token={accessToken}&360buy_param_json={HttpUtility.UrlEncode(buyParamJson360)}";

            return url;
        }

        /// <summary>
        /// 得到签名参数，算法如下：
        /// https://open.jd.com/home/home#/doc/common?listId=890
        /// </summary>
        /// <param name="appSecret"></param>
        /// <param name="buyParamJson360"></param>
        /// <param name="accessToken"></param>
        /// <param name="appKey"></param>
        /// <param name="method"></param>
        /// <param name="timestamp"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public string getSign(string appSecret, string buyParamJson360, string accessToken, string appKey,
            string method, string
                timestamp,
            string v = "1.0")
        {
            // 得到字符串
            var str =
                $"{appSecret}360buy_param_json{buyParamJson360}access_token{accessToken}app_key{appKey}method{method}timestamp{timestamp}v{v}{appSecret}";

            // 使用MD5加密
            MD5 md5 = MD5.Create();
            Byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

            // 把二进制转化为大写的十六进制
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                string hex = bytes[i].ToString("X");
                if (hex.Length == 1)
                {
                    result.Append("0");
                }

                result.Append(hex);
            }

            return result.ToString();
        }
    }

    public class GetJDGoodsList()
    {
    }
}