using System;
using System.Collections.Generic;

namespace ShopAPI.Modals
{
    /// <summary>
    /// 获取京东商品的返回值
    /// </summary>
    public class JdUnionOpenGoodsJingfenQueryResponceModel
    {
        public JdUnionOpenGoodsJingfenQueryResponceInnerModel jd_union_open_goods_jingfen_query_responce { get; set; }

        public class JdUnionOpenGoodsJingfenQueryResponceInnerModel
        {
            public long code { get; set; }
            public string queryResult { get; set; }
        }
    }

    /// <summary>
    /// 上面的 queryResult 结构
    /// </summary>
    public class QueryResult
    {
        public long code { get; set; }
        public string message { get; set; }

        public long totalCount { get; set; }
        public List<JFGoodsResp> data { get; set; }

        public class JFGoodsResp
        {
            public CategoryInfo categoryInfo { get; set; }

            public class CategoryInfo
            {
                public long cid1 { get; set; }
                public string cid1Name { get; set; }
                public long cid2 { get; set; }
                public string cid2Name { get; set; }
                public long cid3 { get; set; }
                public string cid3Name { get; set; }
            }

            public long comments { get; set; }

            public CommissionInfo commissionInfo { get; set; }

            public class CommissionInfo
            {
                public double commission { get; set; }
                public double commissionShare { get; set; }
                public double couponCommission { get; set; }
                public double plusCommissionShare { get; set; }
                public int isLock { get; set; }
                public long startTime { get; set; }
                public long endTime { get; set; }
            }

            public CouponInfo couponInfo { get; set; }

            public class CouponInfo
            {
                public List<Coupon> couponList { get; set; }

                public class Coupon
                {
                    public int bindType { get; set; }
                    public double discount { get; set; }
                    public string link { get; set; }
                    public int platformType { get; set; }
                    public double quota { get; set; }
                    public long getStartTime { get; set; }
                    public long getEndTime { get; set; }
                    public long useStartTime { get; set; }
                    public int hotValue { get; set; }
                }
            }

            public double goodCommentsShare { get; set; }

            public ImageInfo imageInfo { get; set; }

            public class ImageInfo
            {
                public List<UrlInfo> imageList { get; set; }
                public string whiteImage { get; set; }

                public class UrlInfo
                {
                    public string url { get; set; }
                }
            }

            public long inOrderCount30Days { get; set; }

            public string materialUrl { get; set; }

            public PriceInfo priceInfo { get; set; }

            public class PriceInfo
            {
                public double price { get; set; }
                public double lowestPrice { get; set; }
                public int lowestPriceType { get; set; }
                public double lowestCouponPrice { get; set; }
                public double historyPriceDay { get; set; }
            }

            public ShopInfo shopInfo { get; set; }

            public class ShopInfo
            {
                public string shopName { get; set; }
                public long shopId { get; set; }
                public double shopLevel { get; set; }
                public string shopLabel { get; set; }
                public string userEvaluateScore { get; set; }
                public string commentFactorScoreRankGrade { get; set; }
                public string logisticsLvyueScore { get; set; }
                public string logisticsFactorScoreRankGrade { get; set; }
                public string afterServiceScore { get; set; }
                public string afsFactorScoreRankGrade { get; set; }
                public string scoreRankRate { get; set; }
            }

            public long skuId { get; set; }

            public string skuName { get; set; }
            public int isHot { get; set; }
            public long spuid { get; set; }
            public string brandCode { get; set; }
            public string brandName { get; set; }
            public string owner { get; set; }
            public PinGouInfo pinGouInfo { get; set; }

            public class PinGouInfo
            {
                public double pingouPrice { get; set; }
                public long pingouTmCount { get; set; }
                public string pingouUrl { get; set; }
                public long pingouStartTime { get; set; }
                public long pingouEndTime { get; set; }
            }

            public ResourceInfo resourceInfo { get; set; }

            public class ResourceInfo
            {
                public int eliteId { get; set; }
                public string eliteName { get; set; }
            }

            public long inOrderCount30DaysSku { get; set; }

            public SeckillInfo seckillInfo { get; set; }

            public class SeckillInfo
            {
                public double seckillOriPrice { get; set; }
                public double seckillPrice { get; set; }
                public long seckillStartTime { get; set; }
                public long seckillEndTime { get; set; }
            }

            public List<int> jxFlags { get; set; }
            public VideoInfo videoInfo { get; set; }

            public class VideoInfo
            {
            }

            public DocumentInfo documentInfo { get; set; }

            public class DocumentInfo
            {
            }

            public BookInfo bookInfo { get; set; }

            public class BookInfo
            {
            }

            public List<int> forbidTypes { get; set; }

            public int deliveryType { get; set; }
            public SkuLabelInfo skuLabelInfo { get; set; }

            public class SkuLabelInfo
            {
            }

            public PromotionLabelInfoList promotionLabelInfoList { get; set; }

            public class PromotionLabelInfoList
            {
            }
        }
    }
}