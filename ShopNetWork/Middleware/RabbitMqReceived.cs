using Microsoft.AspNetCore.Builder;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace ShopNetWork.Middleware
{
    public static class RabbitMqReceived
    {
        public static IWebHostBuilder UseRabbitMqReceived(this IWebHostBuilder builder)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            var message = "";
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                message = Encoding.UTF8.GetString(body);


            };
            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
            return builder;

        }
    }
}
