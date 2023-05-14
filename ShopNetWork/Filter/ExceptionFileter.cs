using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ShopNetWork.Filter
{
    public class CustomExceptionFileter : IAsyncExceptionFilter
    {
        private readonly ILogger<CustomExceptionFileter> logger;

        public CustomExceptionFileter(ILogger<CustomExceptionFileter> logger)
        {
            this.logger = logger;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            logger.LogError("全局日志记录:" + context.Exception.Message);
            return Task.CompletedTask;
        }
    }
}