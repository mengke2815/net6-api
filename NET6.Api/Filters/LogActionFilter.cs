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
        readonly OperationLogRepository _logRepository;

        public LogActionFilter(IHttpContextAccessor context, OperationLogRepository logRepository)
        {
            _context = context;
            _logRepository = logRepository;
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(a => a.GetType() == typeof(NoLogAttribute)))
            {
                return next();
            }
            return LogAsync(context, next);
        }

        public async Task LogAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sw = new Stopwatch();
            sw.Start();
            var actionResult = (await next()).Result;
            sw.Stop();
            //操作参数
            var args = context.ActionArguments.ToJson();
            //操作结果
            var result = actionResult?.ToJson();

            var request = _context.HttpContext?.Request;
            var ua = request?.Headers["User-Agent"];
            var client = UAParser.Parser.GetDefault().Parse(ua);
            //var device = client.Device.Family.ToLower() == "other" ? "" : client.Device.Family;

            var controller = ((ControllerActionDescriptor)context.ActionDescriptor).ControllerName;
            var action = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;

            var log = new OperationLog
            {
                ApiMethod = context.HttpContext.Request.Method.ToLower(),
                ApiPath = context.ActionDescriptor.AttributeRouteInfo?.Template?.ToLower(),
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                Params = args,
                Result = result,
                Browser = client.UA.ToString(),
                Os = client.OS.ToString(),
                Device = client.Device.ToString(),
                BrowserInfo = ua,
                ApiLabel = $"/{controller}/{action}",
                IP = CommonFun.GetIP(request)
            };

            await _logRepository.AddAsync(log);
        }
    }
}
