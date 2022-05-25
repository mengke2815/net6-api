namespace NET6.Api.Controllers;

/// <summary>
/// 其他功能
/// </summary>
public class OtherController : BaseController
{
    readonly IEventPublisher _eventPublisher;
    public OtherController(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 事件总线
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("pub")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EventPubAsync()
    {
        await _eventPublisher.PublishAsync(new ChannelEventSource(SubscribeEnum.登录事件, "这里是用户登录消息"));
        return JsonView(true);
    }
    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("lock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LockAsync()
    {
        RedisHelper.Set("StockCount", 100);
        Logs($"初始化库存：100件");
        Logs($"模拟抢购数：105人");
        Thread.Sleep(2000);
        //开始模拟并发
        Parallel.For(0, 105, (i) =>
        {
            Task.Run(async () =>
            {
                using var mylock = RedisHelper.Lock("分布式锁名称", 5);
                if (mylock == null) Logs("获取分布式锁失败...");
                #region 执行业务
                var count = RedisHelper.Get<int>("StockCount");
                if (count > 0)
                {
                    count--;
                    RedisHelper.Set("StockCount", count);
                    Logs($"第{i + 1}个人已抢到，当前剩余：{RedisHelper.Get<int>("StockCount")}件");
                    //下单推入消息总线
                    await _eventPublisher.PublishDelayAsync(new ChannelEventSource(SubscribeEnum.下单事件, $"用户{i + 1}的订单开始处理..."), 2500);
                }
                else
                {
                    Logs($"第{i + 1}个人未抢到，商品已抢完...");
                }
                #endregion
            });
        });
        return JsonView(true);
    }
}
