namespace NET6.Api.Subscribers;

/// <summary>
/// 审计日志
/// </summary>
public class LogSubscriber : IEventSubscriber
{
    readonly OperationLogRepository _logRep;
    public LogSubscriber(OperationLogRepository logRep)
    {
        _logRep = logRep;
    }

    [EventSubscribe(SubscribeEnum.审计日志)]
    public async Task LoginEvent(EventHandlerExecutingContext context)
    {
        var log = context.Source.Payload.ToString().ToObject<OperationLog>();
        //分表插入日志
        await _logRep.AddSplitTableAsync(log);
    }
}