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
            Log.Error("执行完毕...");
            return Task.CompletedTask;
        }
    }
}
