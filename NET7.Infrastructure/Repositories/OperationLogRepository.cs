namespace NET7.Infrastructure.Repositories;

/// <summary>
/// 日志仓储
/// </summary>
public class OperationLogRepository : BaseRepository<OperationLog, OperationLogView>
{
    public OperationLogRepository(IMapper mapper, SqlSugarScope sqlSugar) : base(sqlSugar)
    {

    }
}
