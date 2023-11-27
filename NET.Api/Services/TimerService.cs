﻿namespace NET.Api.Services;

/// <summary>
/// 后台任务
/// </summary>
public class TimerService : BackgroundService
{
    readonly OperationLogRepository _operationlogRep;
    public TimerService(OperationLogRepository operationlogRep)
    {
        _operationlogRep = operationlogRep;
    }
    /// <summary>
    /// 执行主方法
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        #region 初始化数据库
        try
        {
            var dbexist = _operationlogRep.CreateDataBase();
            if (dbexist)
            {
                var exist = _operationlogRep.IsTableExist<Address>();
                if (!exist)
                {
                    _operationlogRep.CreateTable(new Type[]
                    {
                        typeof(Address)
                    });
                    Log.Error($"数据库初始化完成...");
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        #endregion

        //初始化socket服务器
        new Thread(new ThreadStart(() =>
        {
            FleckServer.Start();
        })).Start();

        Log.Error("初始化完成...");
        return Task.CompletedTask;
    }
}
