namespace NET.Infrastructure.Tools;

/// <summary>
/// SqlSugar扩展
/// </summary>
public static class SqlSugarExtensions
{
    #region 事务操作
    public static Task BeginTranAsync(this SqlSugarScopeProvider _sqlSugar)
    {
        return _sqlSugar.BeginTranAsync();
    }
    public static Task CommitTranAsync(this SqlSugarScopeProvider _sqlSugar)
    {
        return _sqlSugar.CommitTranAsync();
    }
    public static Task RollbackTranAsync(this SqlSugarScopeProvider _sqlSugar)
    {
        return _sqlSugar.RollbackTranAsync();
    }

    #endregion

    #region 原生Sql
    public static Task<int> ExecuteCommandAsync(this SqlSugarScopeProvider _sqlSugar, string sql)
    {
        return _sqlSugar.Ado.ExecuteCommandAsync(sql);
    }
    public static Task<DataTable> GetDataTableAsync(this SqlSugarScopeProvider _sqlSugar, string sql)
    {
        return _sqlSugar.Ado.GetDataTableAsync(sql);
    }
    public static Task<object> GetScalarAsync(this SqlSugarScopeProvider _sqlSugar, string sql)
    {
        return _sqlSugar.Ado.GetScalarAsync(sql);
    }
    public static Task<List<T>> SqlQueryAsync<T>(this SqlSugarScopeProvider _sqlSugar, string sql)
    {
        return _sqlSugar.Ado.SqlQueryAsync<T>(sql);
    }
    public static ISugarQueryable<T> SqlQueryable<T>(this SqlSugarScopeProvider _sqlSugar, string sql) where T : class, new()
    {
        return _sqlSugar.SqlQueryable<T>(sql);
    }
    #endregion

    #region 泛型CRUD
    public static Task<bool> AnyAsync<T>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().AnyAsync(exp);
    }
    public static ISugarQueryable<T> Query<T>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().Where(a => !a.IsDeleted).Where(exp);
    }
    public static ISugarQueryable<T> Query<T>(this SqlSugarScopeProvider _sqlSugar) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().Where(a => !a.IsDeleted);
    }
    public static Task<Dto> GetDtoAsync<T, Dto>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>().FirstAsync();
    }
    public static ISugarQueryable<Dto> QueryDto<T, Dto>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>();
    }
    public static ISugarQueryable<Dto> QueryDto<T, Dto>(this SqlSugarScopeProvider _sqlSugar) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().Where(a => !a.IsDeleted).Select<Dto>();
    }
    public static Task<int> AddAsync<T>(this SqlSugarScopeProvider _sqlSugar, T entity) where T : EntityBase, new()
    {
        entity.CreateUserId = CurrentUser.UserId;
        CommonFun.CoverNull(entity);
        return _sqlSugar.Insertable(entity).ExecuteCommandAsync();
    }
    public static Task<int> AddAsync<T>(this SqlSugarScopeProvider _sqlSugar, List<T> entities) where T : EntityBase, new()
    {
        foreach (var item in entities)
        {
            item.CreateUserId = CurrentUser.UserId;
        }
        CommonFun.CoverNull(entities);
        return _sqlSugar.Insertable(entities).ExecuteCommandAsync();
    }
    public static Task<T> GetAsync<T>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().Where(exp).FirstAsync();
    }
    public static Task<T> GetAsync<T>(this SqlSugarScopeProvider _sqlSugar, string id) where T : EntityBase, new()
    {
        return _sqlSugar.Queryable<T>().InSingleAsync(id);
    }
    public static Task<int> UpdateAsync<T>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> wherexp, Expression<Func<T, T>> upexp) where T : EntityBase, new()
    {
        return _sqlSugar.Updateable<T>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
    }
    public static Task<int> UpdateAsync<T>(this SqlSugarScopeProvider _sqlSugar, T entity) where T : EntityBase, new()
    {
        return _sqlSugar.Updateable<T>(entity).ExecuteCommandAsync();
    }
    public static Task<int> UpdateAsync<T>(this SqlSugarScopeProvider _sqlSugar, List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.Updateable<T>(entities).ExecuteCommandAsync();
    }
    public static Task<int> DeleteAsync<T>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
    {
        return _sqlSugar.Deleteable<T>().Where(wherexp).ExecuteCommandAsync();
    }
    public static Task<int> DeleteAsync<T>(this SqlSugarScopeProvider _sqlSugar, T entity) where T : EntityBase, new()
    {
        return _sqlSugar.Deleteable<T>(entity).ExecuteCommandAsync();
    }
    public static Task<int> DeleteAsync<T>(this SqlSugarScopeProvider _sqlSugar, List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.Deleteable<T>(entities).ExecuteCommandAsync();
    }
    public static Task<int> SoftDeleteAsync<T>(this SqlSugarScopeProvider _sqlSugar, string id) where T : EntityBase, new()
    {
        return _sqlSugar.Updateable<T>().Where(a => a.Id.Equals(id)).SetColumns(a => new T()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = CurrentUser.UserId
        }).ExecuteCommandAsync();
    }
    public static Task<int> SoftDeleteAsync<T>(this SqlSugarScopeProvider _sqlSugar, Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
    {
        return _sqlSugar.Updateable<T>().Where(wherexp).SetColumns(a => new T()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = CurrentUser.UserId
        }).ExecuteCommandAsync();
    }
    #endregion

    #region 大数据写入
    public static Task<int> AddBulkAsync<T>(this SqlSugarScopeProvider _sqlSugar, List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.Fastest<T>().BulkCopyAsync(entities);
    }
    public static Task<int> UpdateBulkAsync<T>(this SqlSugarScopeProvider _sqlSugar, List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.Fastest<T>().BulkUpdateAsync(entities);
    }
    public static Task<int> AddSplitTableBulkAsync<T>(this SqlSugarScopeProvider _sqlSugar, List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.Fastest<T>().SplitTable().BulkCopyAsync(entities);
    }
    #endregion
}
