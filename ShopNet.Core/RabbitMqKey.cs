using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.Core
{
    public static class GoodsAddDataMq
    {
        public const string exchangeName = "DataAnalyticeExchange";
        public const string routingKey = "RealTimeDataKey";
        public const string queueName = "RealTimeDataQueue";
        public const string exchangeType = ExchangeType.Direct;
    }
}