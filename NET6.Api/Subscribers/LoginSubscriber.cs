namespace NET6.Api.Subscribers;

/// <summary>
/// 订阅者
/// </summary>
public class LoginSubscriber : IEventSubscriber
{
    [EventSubscribe(SubscribeEnum.登录事件)]
    public async Task LoginEvent(EventHandlerExecutingContext context)
    {
        Log.Error($"事件总线：{context.Source.Payload}");
        await Task.CompletedTask;
    }
}
