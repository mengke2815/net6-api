namespace NET7.Api.Filters;

/// <summary>
/// 日志过滤器
/// </summary>
public class LogActionFilter : IAsyncActionFilter
{
    readonly IEventPublisher _eventPublisher;
    public LogActionFilter(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
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
        var result = ((ObjectResult)actionResult)?.Value?.ToJson();
        var request = BuilderExtensions.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
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

        //解析xml注释
        var cName = ((ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo.FullName;
        var mName = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;
        var xml = BuilderExtensions.ServiceProvider.GetRequiredService<XElement>();
        var members = xml.Elements().FirstOrDefault(a => a.Name == "members").Elements();
        var param = ((ControllerActionDescriptor)context.ActionDescriptor).Parameters;
        var paramList = new List<string>();
        foreach (var item in param)
        {
            paramList.Add(item.ParameterType.FullName);
        }
        var pm = "";
        if (paramList.Count > 0)
        {
            pm = $"({string.Join(',', paramList)})";
        }
        var cDesc = members.FirstOrDefault(a => a.FirstAttribute.Value == $"T:{cName}").Elements().FirstOrDefault(a => a.Name == "summary").Value.Trim();
        var mDesc = members.FirstOrDefault(a => a.FirstAttribute.Value == $"M:{cName}.{mName}Async{pm}").Elements().FirstOrDefault(a => a.Name == "summary").Value.Trim();


        //推入消息总线
        await _eventPublisher.PublishAsync(SubscribeEnum.审计日志, log.ToJson());
    }
}
