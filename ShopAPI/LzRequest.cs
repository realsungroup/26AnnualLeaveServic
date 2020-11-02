using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace ShopAPI {
    public class HttpCommonReponseModals {
        // public 

    }

    public class LzRequest {
        public LzRequest (string baseURL) {
            this.baseURL = baseURL;
        }

        public string baseURL = "";

        private string getReqURL (string baseURL, string url) {
            return baseURL + url;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public async Task<object> login (string username, string password) {
            var url = "/api/Account/Login";
            var reqURL = getReqURL (baseURL, url);

            var res = await reqURL
                .WithHeaders (new { Content_Type = "application/json" })
                .PostJsonAsync (new {
                    code = username,
                        password = password
                }).ReceiveJson ();
            return res;
        }

    }
}