namespace ShopNetWork.Extensions
{
    public class TokenConfig
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public static string secret = "9999999999999999999999999";
        /// <summary>
        /// 签发者
        /// </summary>
        public static string issuer = "yinmingneng";
        /// <summary>
        /// 受众
        /// </summary>
        public static string audience = "ymn";
        /// <summary>
        /// 令牌过期时间
        /// </summary>
        public static int accessExpiration = 30;
    }
}
