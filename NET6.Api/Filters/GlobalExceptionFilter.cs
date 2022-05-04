using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NET6.Domain.ViewModels;
using NET6.Infrastructure.Tools;
using Serilog;

namespace NET6.Api.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        readonly IWebHostEnvironment hostEnvironment;
        public GlobalExceptionFilter(IWebHostEnvironment _hostEnvironment)
        {
            hostEnvironment = _hostEnvironment;
        }
        public void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled)//如果异常没有处理
            {
                var result = new JsonView
                {
                    Code = 500,
                    Msg = "服务器发生未处理的异常"
                };

                if (hostEnvironment.IsDevelopment())
                {
                    result.Msg += "：" + context.Exception.Message;
                    result.Data = context.Exception.StackTrace;
                }

                Log.Error(result.ToJson());

                context.Result = new JsonResult(result);
                context.ExceptionHandled = true;//异常已处理
            }
        }
    }
}
