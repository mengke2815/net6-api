using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NET6.Api.Attributes;
using NET6.Domain.Entities;
using NET6.Infrastructure.Repositories;
using NET6.Infrastructure.Tools;
using System.Diagnostics;

namespace NET6.Api.Filters
{
    /// <summary>
    /// 日志过滤器
    /// </summary>
    public class LogActionFilter : IAsyncActionFilter
    {
        readonly IHttpContextAccessor _context;
        readonly OperationLogRepository _logRep;

        public LogActionFilter(IHttpContextAccessor context, OperationLogRepository logRep)
        {
            _context = context;
            _logRep = logRep;
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(a => a.GetType() == typeof(NoLogAttribute)))
            {
                return next();
            }
            return LogAsync(context, next);
        }

        private async Task LogAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sw = new Stopwatch();
            sw.Start();
            var actionResult = (await next()).Result;
            sw.Stop();

            var args = context.ActionArguments.ToJson();
            var result = actionResult?.ToJson();
            var request = _context.HttpContext?.Request;
            var ua = request?.Headers["User-Agent"];
            var client = UAParser.Parser.GetDefault().Parse(ua);
            var controller = ((ControllerActionDescriptor)context.ActionDescriptor).ControllerName.ToLower();
            var action = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName.ToLower();

            var log = new OperationLog
            {
                ApiMethod = context.HttpContext.Request.Method.ToLower(),
                ApiPath = $"/{controller}/{action}",
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                Params = args,
                Result = result,
                Browser = client.UA.ToString(),
                Os = client.OS.ToString(),
                Device = client.Device.ToString(),
                BrowserInfo = ua,
                IP = CommonFun.GetIP(request)
            };

            await _logRep.AddAsync(log);
        }
    }
}
