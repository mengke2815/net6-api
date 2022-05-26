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

    [EventSubscribe(SubscribeEnum.下单事件)]
    public async Task OrderEvent(EventHandlerExecutingContext context)
    {
        Log.Error($"事件总线：{context.Source.Payload}");
        await Task.CompletedTask;
    }
}

#region 对事件总线添加redis支持
public class RedisEventSource : IEventSource
{
    /// <summary>
    /// 事件 Id
    /// </summary>
    public string EventId { get; set; }
    /// <summary>
    /// 事件承载（携带）数据
    /// </summary>
    public object Payload { get; set; }
    /// <summary>
    /// 取消任务 Token
    /// </summary>
    /// <remarks>用于取消本次消息处理</remarks>
    public CancellationToken CancellationToken { get; set; }
    /// <summary>
    /// 事件创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}
public class RedisEventSourceStorer : IEventSourceStorer
{
    readonly SemaphoreSlim _semaphore = new(0);
    public async ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken)
    {
        await RedisHelper.LPushAsync("MyEventBus", eventSource);
        _semaphore.Release();
    }
    public async ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return await RedisHelper.RPopAsync<RedisEventSource>("MyEventBus");
    }
}
#endregion