using System;

namespace ShopAPI
{
    public class Constant
    {
        /// <summary>
        /// 后端相关
        /// </summary>
        // 后端 api 基地址
        public static string realsunBaseURL = "http://kingofdinner.realsun.me:5201/";

        // 后端 api 登录用户名称
        public static string realsunUsername = "demo1";

        // 后端 api 登录密码
        public static string realsunPassword = "123456";

        // 访问后端的 accessToken
        public static string realsunAccessToken = "";

        /// <summary>
        /// 淘宝相关
        /// </summary>
        // 淘宝 appKey
        public static string appkey = "31684648";

        // 淘宝 appSecret
        public static string appsecret = "90ca4b896bed288aeacaf4a4e7c129e3";

        /// <summary>
        /// 京东相关
        /// </summary>
        // 京东 appKey
        public static string jdAppKey = "c64663d5060b81be4d7e9c55d71ee127";
        // 京东 appSecret
        public static string jdAppSecret = "2325399787254347a27a053d7aa18573";
        

        /// <summary>
        /// 表 resid
        /// </summary>
        // 物料ID表
        public static string materialResid = "657651132241";

        // 商户设置表
        public static string commercialTenantSetResid = "657632198616";

        // 商品明细表
        public static string goodsResid = "650990015567";

        // 商品明细表-不包含下架
        public static string notUnderGoodsResid = "658233019592";

        // 商品明细表-不包含已上架已下架（isPutaway 值为空的商品）
        public static string nullGoodsResid = "658253937383";

        // 商铺表
        public static string shopResid = "650991724130";

        // 上架表
        public static string groundingResid = "650998291867";
    }
}