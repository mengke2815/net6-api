namespace NET6.Api.Subscribers;

/// <summary>
/// 订阅者
/// </summary>
public class LoginSubscriber : IEventSubscriber
{
    [EventSubscribe("Login")]
    public async Task LoginEvent(EventHandlerExecutingContext context)
    {
        Log.Error($"事件总线：{context.Source.Payload}");
        await Task.CompletedTask;
    }
}
