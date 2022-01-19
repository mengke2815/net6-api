using NET6.Domain.Entities;
using NET6.Domain.ViewModels;
using SqlSugar;

namespace NET6.Infrastructure.Repositories
{
    /// <summary>
    /// 日志仓储
    /// </summary>
    public class OperationLogRepository : BaseRepository<OperationLog, OperationLogView>
    {
        public OperationLogRepository(SqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
