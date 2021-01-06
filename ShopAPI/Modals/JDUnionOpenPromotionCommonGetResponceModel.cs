using System;

namespace ShopAPI.Modals
{
    /// <summary>
    /// 京东获取推广链接接口返回值
    /// </summary>
    public class JDUnionOpenPromotionCommonGetResponceModel
    {
        public JDUnionOpenPromotionCommonGetResponce jd_union_open_promotion_common_get_responce { get; set; }

        public class JDUnionOpenPromotionCommonGetResponce
        {
            public string code { get; set; }
            public string getResult { get; set; }
        }


        public class GetResult
        {
            public string code { get; set; }
            public string message { get; set; }
            public Data data { get; set; }

            public class Data
            {
                public string clickURL { get; set; }
            }
        }
    }
}