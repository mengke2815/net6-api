﻿namespace NET7.Api.Subscribers;

/// <summary>
/// 对事件总线添加redis支持
/// </summary>
public class RedisEventSourceStorer : IEventSourceStorer
{
    readonly Channel<IEventSource> _channel;
    public RedisEventSourceStorer(int capacity = 5000)
    {
        _channel = Channel.CreateBounded<IEventSource>(capacity);
        RedisHelper.SubscribeList("MyEventBus", msg =>
        {
            if (msg.NotNull())
            {
                _channel.Writer.TryWrite(msg.ToObject<ChannelEventSource>());
            }
        });
    }
    public async ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken)
    {
        await RedisHelper.LPushAsync("MyEventBus", eventSource.ToJson());
    }
    public async ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }

    #region redis实现
    //static readonly SemaphoreSlim _semaphore = new(0);
    //public async ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken)
    //{
    //    await RedisHelper.LPushAsync("MyEventBus", eventSource);
    //    _semaphore.Release();
    //}
    //public async ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken)
    //{
    //    await _semaphore.WaitAsync(cancellationToken);
    //    return await RedisHelper.RPopAsync<ChannelEventSource>("MyEventBus");
    //}
    //public void Dispose()
    //{
    //    _semaphore.Release(int.MaxValue);
    //    _semaphore.Dispose();
    //} 
    #endregion
}
