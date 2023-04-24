using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SqlSugar;
using System;
using System.Text;

namespace ShopNetWork.Controllers
{
    /// <summary>
    /// 电话发短信控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : ControllerBase
    {
        
        private readonly ISqlSugarClient db;
       

        public SMSController(ISqlSugarClient db)
        {
            this.db = db;
        }

        [HttpGet("PhoneMessage")]
        public IActionResult PhoneMessage()
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
                Console.WriteLine(" [x] Received {0}", message);
            };

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

            
            return Ok(message);

        }
    }
}
