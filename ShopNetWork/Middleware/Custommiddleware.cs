using Microsoft.AspNetCore.Http;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Text;

namespace ShopNetWork.Middleware
{
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;

        public MyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // 执行自定义中间件逻辑
            await context.Response.WriteAsync("H from MyMiddleware!");

            // 调用下一个中间件
            await _next(context);
        }
    }
}