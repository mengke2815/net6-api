namespace NET6.Api.Subscribers;

/// <summary>
/// 对事件总线添加redis支持
/// </summary>
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
        return await RedisHelper.RPopAsync<ChannelEventSource>("MyEventBus");
    }
}
