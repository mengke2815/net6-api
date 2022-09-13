namespace NET6.Api.Subscribers;

/// <summary>
/// 订阅者
/// </summary>
public class LoginSubscriber : IEventSubscriber
{
    readonly AddressRepository _addressRep;
    public LoginSubscriber(AddressRepository addressRep)
    {
        _addressRep = addressRep;
    }

    [EventSubscribe(SubscribeEnum.登录事件)]
    public async Task LoginEvent(EventHandlerExecutingContext context)
    {
        var s = await _addressRep.Query().FirstAsync();
        Log.Error($"事件总线：{context.Source.Payload}_{s.Name}");
    }
}