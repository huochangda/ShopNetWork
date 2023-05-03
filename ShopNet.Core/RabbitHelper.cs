using MathNet.Numerics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt;
using NPOI.SS.Formula.Functions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopNet.Common;
using System;
using System.Diagnostics.Metrics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShopNet.Core
{
    /// <summary>
    /// 企业级rabbitmq使用
    /// </summary>
    public class RabbitHelper
    {
        private IConnection connection = null;

        private IModel channel = null;

        private CancellationTokenSource _cancellationTokenSource;

        private Task _consumerTask;

        private static RabbitHelper _RabbitMQClient = null;

        public static RabbitHelper rabbitmq
        {
            get
            {
                if (_RabbitMQClient == null)
                {
                    lock (new object())
                    {
                        if (_RabbitMQClient == null)
                        {
                            _RabbitMQClient = new RabbitHelper();
                        }
                    }
                }
                return _RabbitMQClient;
            }
        }

        //public event EventHandler<string> Received;
        public RabbitHelper()
        {
            //获取RabbitMQ配置映射对象
            //LogHelper.Info("config:" + config.ToJson());
            //创建连接工厂
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = AppSettings.Configuration["RabbitMqServer:UserName"],//用户名
                Password = AppSettings.Configuration["RabbitMqServer:Password"],//密码
                HostName = AppSettings.Configuration["RabbitMqServer:HostName"],//ip地址
                                                                                //Port = config.Port,//端口号
                                                                                //VirtualHost = config.VirtualHost//虚拟主机 如果不指定该参数则默认访问根目录虚拟主机"/"
            };

            factory.AutomaticRecoveryEnabled = true;   //设置端口后自动恢复连接属性
                                                       //LogHelper.Info("factory:" + factory.ToJson());
                                                       //创建连接
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="routingKey">路由key</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="content">消息内容</param>
        /// <param name="exchangeType">交换机类型，默认：ExchangeType.Direct</param>
        public void Send<T>(string exchangeName, string routingKey, string queueName, T content, string exchangeType = ExchangeType.Direct)
        {
            if (content == null) return;
            try
            {
                lock (new object())
                {
                    //创建交换机
                    channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true, autoDelete: false, arguments: null);//durable交换机持久化 autoDelete在消费完成后自动删除交换机 arguments额外附加参数
                                                                                                                                           //创建队列
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);//durable队列持久化 exclusive独占队列(只允许当前连接和通道使用队列) autoDelete在消费完成后自动删除队列 arguments额外附加参数
                                                                                                                                //将队列绑定到交换机
                    channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey, arguments: null);
                    //设置消费者每次可以消费的消息数量
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);//prefetchCount：允许消费者最多同时消费几个消息 设置为1 防止因消费者宕机导致消息丢失(也可以设置多条提高消费速度)
                                                                                       //消息的额外设置
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;//消息持久化
                                                 //properties.Expiration = "5000";//消息过期时间 建议在auguments参数中设置过期时间
                                                 //properties.Priority = 99;//消息优先级 需要在声明队列时为arguments参数赋值{ "x-max-priority", 99 } 队列支持的最大优先级数;如果未设置，队列将不支持消息优先级。
                                                 //消息内容 字节类型
                    byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));
                    //发送消息
                    channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The operation has timed out"))
                {
                    Thread.Sleep(20);//RabbitMQ连接超时,线程休眠后重新发送消息:
                    Send(exchangeName, routingKey, queueName, content, exchangeType);
                }

                if (ex.Message.Contains("Pipelining of requests forbidden"))
                {
                    Thread.Sleep(20);//RabbitMQ多线程同时使用一个信道,线程休眠后重新发送消息
                    Send(exchangeName, routingKey, queueName, content, exchangeType);
                }
            }
        }

        #region 死信队列

        //////////////////////死信队列//////////////////////
        //说明：当消息处理异常时可以把消息放入到死信队列中以便后期处理 例：下面的代码设置了消息的存活时间与队列最多存储消息条数 当消息过期或队列超过10条消息后便会将消息加入到死信队列中
        //流程：
        //1.声明死信队列的交换机，路由key，队列并绑定
        //channel.ExchangeDeclare("DLXexchange", type: "fanout", durable: true, autoDelete: false);
        //channel.QueueDeclare("DLXqueue", durable: true, exclusive: false, autoDelete: false);
        //channel.QueueBind("DLXqueue", "DLXexchange", "DLXroutkey");
        ////2.声明当前队列的交换机，队列，路由key，声明队列时需要为arguments参数赋值，将死信队列与当前队列绑定(绑定死信交换机与路由key即可)
        //channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true, autoDelete: false, arguments: null);
        //channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object> {
        //     { "x-dead-letter-exchange","DLXexchange"},
        //     { "x-dead-letter-routing-key","DLXroutkey"},
        //     { "x-message-ttl",100000},
        //     { "x-max-length", 10}
        //     });
        //channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey, arguments: null);
        //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        ////3.进行消息设置并发送消息
        //var properties = channel.CreateBasicProperties();
        //properties.Persistent = true;
        //byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));
        //channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);

        ////////////////////auguments参数详解//////////////////
        //Dictionary<string, object> auguments = new Dictionary<string, object>();
        //auguments.Add("x-dead-letter-exchange", "DLXexchange"); //设置DLX(死信交换机)
        //auguments.Add("x-dead-letter-routing-key", "DLXroutkey"); //设置DLK(死信路由key)
        //auguments.Add("x-message-ttl", 5000); //设置TTL(消息的存活时间) 5秒
        //auguments.Add("x-expires", 60000); //队列在60秒内未被任何形式消费则被删除
        //auguments.Add("x-max-priority", 99); //队列支持的最大优先级数 如果未设置队列将不支持消息优先级 99为最大优先级
        //auguments.Add("x-max-length", 10); //队列中最多能存储多少条消息 10条 当超过10条后最先入队的消息将会被删除或加入到死信队列中
        //auguments.Add("x-max-length-bytes", 5 * 1024); //加入队列中消息的最大容积 2M
        //auguments.Add("x-queue-mode", "lazy");//懒队列，先将消息保存到磁盘上，当消息被消费的时候才会加载到内存中，设置后面对高并发可以减轻rabbitmq服务器的压力(但会降低响应速度，依据业务场景考虑是否使用懒队列)，与消息持久化配合使用。

        #endregion 死信队列

        public void Receive(string exchangeName, string routingKey, string queueName, Action<string> callback, string exchangeType = ExchangeType.Direct, Action errorCallback = null, CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _consumerTask = Task.Run(() =>
            {
                //在消费端重新进行一次交换机和队列的声明与绑定 防止消费端在生产端之前运行产生问题
                //需要死信队列时需像发送方法一样先声明死信队列
                channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                //事件基本消费者
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    string message = Encoding.UTF8.GetString(ea.Body.ToArray());//消息内容
                    try
                    {
                        callback(message);//接收消息后的回调函数 做相应的业务处理
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);//手动确认消息 在处理完业务后执行 此时会从队列中删除该消息 deliveryTag：确认队列中的哪个具体消息被消费 multiple:是否开启批量消息确认
                    }
                    catch (Exception ex)
                    {
                        //确认消费失败 requeue参数设置为true会将消费失败的消息重新放回队列头部(可能导致死循环)
                        errorCallback?.Invoke();//出错后的回调函数
                        throw ex;
                    }
                };
                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);//autoAck:是否开启消息的自动应答(如开启自动应答，则无需上面的channel.BasicAck()代码)
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Task.Delay(100, _cancellationTokenSource.Token).Wait();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 处理死信队列消息
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="queueName"></param>
        /// <param name="callback"></param>
        /// <param name="exchangeType"></param>
        /// <param name="errorCallback"></param>
        /// <param name="cancellationToken"></param>
        public void ReceiveDeadLetter(string exchangeName, string routingKey, string queueName, Action<string> callback, string exchangeType = ExchangeType.Direct, Action errorCallback = null, CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _consumerTask = Task.Run(() =>
            {
                //声明死信队列的交换机，路由key，队列并绑定
                channel.ExchangeDeclare("DLXexchange", type: "fanout", durable: true, autoDelete: false);
                channel.QueueDeclare("DLXqueue", durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind("DLXqueue", "DLXexchange", "DLXroutkey");
                //创建消费者
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    string message = Encoding.UTF8.GetString(ea.Body.ToArray());//消息内容
                    try
                    {
                        callback(message);//接收消息后的回调函数 做相应的业务处理
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);//手动确认消息 在处理完业务后执行 此时会从队列中删除该消息 deliveryTag：确认队列中的哪个具体消息被消费 multiple:是否开启批量消息确认
                    }
                    catch (Exception ex)
                    {
                        //确认消费失败 requeue参数设置为true会将消费失败的消息重新放回队列头部(可能导致死循环)
                        errorCallback?.Invoke();//出错后的回调函数
                        throw ex;
                    }
                };
                channel.BasicConsume(queue: "DLXqueue", autoAck: false, consumer: consumer);
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Task.Delay(100, _cancellationTokenSource.Token).Wait();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 关闭RabbitMq连接
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (channel != null) channel.Dispose();
                if (connection != null) connection.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        public void StopConsuming()
        {
            _cancellationTokenSource?.Cancel();
            _consumerTask?.Wait();
        }
    }
}