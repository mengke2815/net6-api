namespace NET7.Api.Subscribers;

/// <summary>
/// 下单事件
/// </summary>
public class OrderSubscriber : IEventSubscriber
{
    [EventSubscribe(SubscribeEnum.下单事件)]
    public async Task OrderEvent(EventHandlerExecutingContext context)
    {
        Log.Error($"事件总线：{context.Source.Payload}");
        await Task.CompletedTask;
    }
}