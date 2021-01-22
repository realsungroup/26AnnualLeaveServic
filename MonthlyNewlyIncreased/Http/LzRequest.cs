using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using static System.Console;
using System.Threading.Tasks;
// using FastJSON;
using Newtonsoft.Json;

namespace MonthlyNewlyIncreased.Http {
    public class HttpCommonReponseModals {
        // public 
    }
    /// <summary>
    /// getTable 方法 options 参数 Modal
    /// </summary>
    public class GetTableOptionsModal {
        public string subresid { get; set; }
        public string cmswhere { get; set; }
        public string hostrecid { get; set; }
        public string sortOrder { get; set; }
        public string sortField { get; set; }
        public string key { get; set; }
        public string cmscolumns { get; set; }
        public string getcolumninfo { get; set; }
        public string pageIndex { get; set; }
        public string pageSize { get; set; }
        public string subcmscolumns { get; set; }
    }

    /// <summary>
    /// getTable 接口响应的数据结构
    /// </summary>
    /// <typeparam name="T">data 每一项的类型</typeparam>
    public class GetTagbleResponseModal<T> {
        public List<T> data { get; set; }
        public long total { get; set; }
        public object error { get; set; }

        public string token { get; set; }
        public string message { get; set; }
        public object ResouceData { get; set; }
    }

    public class LzRequest {
        public LzRequest (string baseURL, Hashtable headers = null) {
            this.baseURL = baseURL;
            this.headers = headers;
        }
        /// <summary>
        /// 基地址
        /// </summary>
        public string baseURL = "";

        /// <summary>
        /// header
        /// </summary>
        public object headers = null;

        /// <summary>
        /// 设置请求的 headers
        /// </summary>
        /// <param name="headers"></param>
        public void setHeaders (object headers) {
            this.headers = headers;
        }

        private string getReqURL (string baseURL, string url) {
            var _baseURL = baseURL;
            // var lastStr = baseURL[baseURL.Length].ToString ();
            // if (lastStr != "/") {
            //     _baseURL += "/";
            // }
            return _baseURL + url;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public async Task<dynamic> login (string username, string password) {
            var url = "api/Account/Login";
            var reqURL = getReqURL (baseURL, url);

            var res = await reqURL
                .WithHeaders (new { Content_Type = "application/json" })
                .PostJsonAsync (new {
                    code = username,
                        password = password
                }).ReceiveJson ();
            return res;
        }

        /// <summary>
        /// 获取表记录
        /// </summary>
        /// <param name="resid">主表 resid</param>
        /// <param name="options">选项</param>
        /// <returns></returns>
        public async Task<GetTagbleResponseModal<T>> getTable<T> (string resid, GetTableOptionsModal options = null) {
            var url = "api/100/table/Retrieve?resid=" + resid;
            var reqURL = getReqURL (baseURL, url);
            var query = new Hashtable ();

            if (options != null) {
                Type t = options.GetType ();
                PropertyInfo[] PropertyList = t.GetProperties ();
                foreach (PropertyInfo item in PropertyList) {
                    string name = item.Name;
                    var value = (string) item.GetValue (options, null);
                    query.Add (name, value);
                }
            }
            var res = await reqURL
                .WithHeaders (headers)
                .SetQueryParams (query)
                .GetJsonAsync<GetTagbleResponseModal<T>> ();

            return res;
        }

        public delegate Task<T> AddRecordsDel<T> (string resid, object data);

        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="resid"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> AddRecords<T> (string resid, object data) {
            var url = "api/100/table/Save";
            var reqURL = getReqURL (baseURL, url);

            var res = await reqURL
                .WithHeaders (headers)
                .PostJsonAsync (new {
                    resid,
                    data = JsonConvert.SerializeObject (data)
                }).ReceiveJson<T> ();
            return res;
        }

    }
}