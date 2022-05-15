namespace NET6.Infrastructure.Repositories;

/// <summary>
/// 日志仓储
/// </summary>
public class OperationLogRepository : BaseRepository<OperationLog, OperationLogView>
{
    public OperationLogRepository(IHttpContextAccessor context, SqlSugarScope sqlSugar) : base(context, sqlSugar)
    {

    }
}
