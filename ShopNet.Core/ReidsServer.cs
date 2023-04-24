using CSRedis;
using ShopNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.Core
{
    /// <summary>
    /// 注入CSRedis服务
    /// </summary>
    public class ReidsServer
    {
        public static class RedisServer
        {
            public static CSRedisClient Cache;
            public static CSRedisClient Sequence;
            public static CSRedisClient Session;

            public static void Initalize()
            {
                Cache = new CSRedisClient(AppSettings.Configuration["RedisServer:Cache"]);
                Sequence = new CSRedisClient(AppSettings.Configuration["RedisServer:Sequence"]);
                Session = new CSRedisClient(AppSettings.Configuration["RedisServer:Session"]);
            }
        }
    }
}
