using System.Collections;
using System.Threading.Tasks;
using static System.Console;
using ShopAPI.Http;
using static ShopAPI.Constant;
using System.Collections.Generic;
using ShopAPI.Modals;
using ShopAPI.Tasks;
using static ShopAPI.Utils;

namespace ShopAPI.Jobs
{
    /// <summary>
    /// 京东商品下架任务
    /// </summary>
    public class UndercarriageJDJob
    {
        private static int prevGoodsCount = -1;

        /// <summary>
        /// 是否需要下架
        /// 为什么需要这样判断？
        /// 因为有一次获取到可以下架的商品数量为 1，但将这条商品进行下架后，下次获取商品，还是能够获取到这条商品，所以就导致不断的去获取要下架的商品和不断的下架，导致后台 cpu 100%卡死。所以加了一个判断，来避免这种情况
        /// </summary>
        /// <param name="goodsCount">商品数量</param>
        /// <returns></returns>
        private static bool isGrounding(int goodsCount)
        {
            // 没有需要下架的商品
            if (goodsCount == 0)
            {
                return false;
            }

            // 上一次上架的商品等于这次上架的商品 且
            // 商品数量少于每次获取的 100 条商品
            if (prevGoodsCount == goodsCount && goodsCount < 100)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <returns></returns>
        public static async Task<object> start()
        {
            isRun = true;
            var ret = new Hashtable();

            var task = new GetUndercarriageGoodsListTask();
            var res = await task.run("jd");

            var goodsList = new List<GroundingTableModal>();

            foreach (var resItem in res)
            {
                var shopID = resItem.shopID;
                // 转换下架商品
                var result = DataCovertTask.goodsTalbe2GroundingTable(resItem.goodsList, "N", shopID);
                goodsList.AddRange(result);
            }

            WriteLine("开始下架京东商品：");
            WriteLine("下架商品数量：" + goodsList.Count);

            var canGrounding = isGrounding(goodsList.Count);

            // 下架商品
            if (canGrounding)
            {
                await undercarriageGoods(goodsList);
                prevGoodsCount = goodsList.Count;
                // 等 1 毫秒后再下架商品
                System.Timers.Timer t = new System.Timers.Timer(1);
                t.Elapsed += new System.Timers.ElapsedEventHandler(timeout);
                t.AutoReset = false;
                t.Enabled = true;
                isRun = false;
            }
            else
            {
                // 等 10 分钟后再下架商品
                System.Timers.Timer t = new System.Timers.Timer(10 * 60 * 1000);
                t.Elapsed += new System.Timers.ElapsedEventHandler(timeout);
                t.AutoReset = false;
                t.Enabled = true;
                isRun = false;
            }

            ret.Add("下架的京东商品数量：", goodsList.Count);

            return ret;
        }

        // 是否正在运行上架的任务
        public static bool isRun = false;

        // 倒计时事件
        public static void timeout(object source, System.Timers.ElapsedEventArgs e)
        {
            // 继续上架商品
            if (!isRun)
            {
                start();
            }
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        public static async Task<object> undercarriageGoods(List<GroundingTableModal> goodsList)
        {
            WriteLine("开始下架京东商品：");

            var client = new LzRequest(realsunBaseURL);
            client.setHeaders(new {Accept = "application/json", accessToken = realsunAccessToken});

            var list = List2TwoDimensionList<GroundingTableModal>(goodsList, 20);
            var ret = new List<object>();
            var index = 1;
            foreach (var itemList in list)
            {
                WriteLine($"{index++} 正在下架的京东商品数量:" + itemList.Count);
                try
                {
                    await client.AddRecords<object>(groundingResid, itemList);
                }
                catch (System.Exception ex)
                {
                    WriteLine("下架京东商品报错：" + ex.Message);
                }
            }

            return ret;
        }
    }
}