namespace ShopNetWork.Extensions
{
    /// <summary>
    /// jwt配置类
    /// </summary>
    public class TokenConfig
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public static string secret = "999999999999999999999";
        /// <summary>
        /// 签发者
        /// </summary>
        public static string issuer = "huochangda";
        /// <summary>
        /// 受众
        /// </summary>
        public static string audience = "hcd";
        /// <summary>
        /// 令牌过期时间
        /// </summary>
        public static int accessExpiration = 30;
    }
}
