using MathNet.Numerics;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NPOI.HSSF.Util.HSSFColor;

namespace ShopNet.Core
{
    public class RabbitConsumer : IConsumer
    {
        private readonly ILogger<RabbitConsumer> _logger;
        private readonly AutoResetEvent _consumerStopped = new AutoResetEvent(false);

        public event EventHandler<string> Received;

        public void Receive()
        {
            RabbitHelper rabbitHelper = new RabbitHelper();

            Task.Run(() =>
            {
                var factory = new ConnectionFactory//类似连接数据库连接
                {
                    HostName = "localhost",//主机名
                    UserName = "guest",//用户名
                    Password = "guest"//密码
                };
                var message = "";
                using var connection = factory.CreateConnection();//创建rabbit连接,使用using关键字确保在离开using块时自动释放连接。
                using var channel = connection.CreateModel();//使用连接对象创建一个新的通道（Channel）
                channel.ExchangeDeclare("myexchange", ExchangeType.Direct);
                channel.QueueDeclare("myqueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind("myqueue", "myexchange", "myroutingkey");

                //channel.QueueDeclare(queue: "hello2", durable: true, exclusive: false, autoDelete: false, arguments: null);
                ///声明一个名为“hello2”的队列。如果队列不存在，则创建它；如果已经存在，则不会进行任何操作。在这里，我们指定该队列为持久化队列。

                #region 参数解析

                //queue: "hello2" 指定要声明的队列的名称。
                //durable: true 若设为 true，则在RabbitMQ服务器重新启动后，队列仍然存在。如果设置为false，则队列将在服务器重启时被删除
                //exclusive: false 若设为true，则只有当前连接可以访问该队列。如果设置为false，则多个连接可以共享同一个队列。
                //autoDelete: false 若设为true，则当与队列相关联的所有消费者都停止使用此队列时，队列将被自动删除。如果设置为false，则队列将一直保留，直到显式删除为止。
                //arguments: null  可选参数，用于设置其他与队列相关的属性。例如，您可以为队列设置消息过期时间、最大长度等。
                //其他参数
                //Message TTL：消息过期时间（Time to Live），指定消息在队列中存储的最长时间。
                //Queue TTL：队列过期时间，指定队列在没有任何消费者连接时存活的最长时间。
                //Max length：队列的最大长度限制，指定队列中可以存储的消息数量上限。
                //Dead - letter exchange：指定死信（Dead Letter）的交换机名称，用于处理无法被消费者正确处理的消息。
                //Dead - letter routing key：指定死信的路由键，用于将死信重新路由到其他队列中。

                #endregion 参数解析

                var consumer = new EventingBasicConsumer(channel);//在指定通道创建消费者实例
                consumer.Received += (model, ea) =>//注册一个事件处理程序用于接收从队列中获取的消息
                {
                    var body = ea.Body.ToArray();
                    message = Encoding.UTF8.GetString(body);
                    Received?.Invoke(this, message);//并使用Received事件将其传递到调用方
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);//调用BasicAck方法确认消息已经被处理并从队列中删除。
                };

                channel.BasicConsume(queue: "myqueue", autoAck: false, consumer: consumer);
                //启动一个基础消费者来接收指定队列中的消息
                //设置autoAck参数为false，手动确认消息处理完成
                while (!_consumerStopped.WaitOne(1000))
                {
                    // Do nothing, waiting for messages
                }
            });
        }

        public void Stop()
        {
            _consumerStopped.Set();
        }
    }
}