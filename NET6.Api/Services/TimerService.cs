using NET6.Infrastructure.Fleck;
using Serilog;

namespace NET6.Api.Services
{
    /// <summary>
    /// 后台任务
    /// </summary>
    public class TimerService : BackgroundService
    {
        /// <summary>
        /// 执行主方法
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //初始化socket服务器
            FleckServer.Start();
            Log.Error("执行完毕...");
            return Task.CompletedTask;
        }
    }
}
