using NET6.Domain.Entities;
using SqlSugar;

namespace NET6.Infrastructure.Repositories
{
    public class OperationLogRepository : BaseRepository<OperationLog, OperationLog>
    {
        public OperationLogRepository(SqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
