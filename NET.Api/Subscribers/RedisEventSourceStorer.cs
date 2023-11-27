namespace NET.Api.Subscribers;

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
}
