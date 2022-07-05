namespace NET6.Infrastructure.Repositories;

/// <summary>
/// 仓储基类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TDto"></typeparam>
public class BaseRepository<TEntity, TDto> where TEntity : EntityBase, new()
{
    readonly IHttpContextAccessor _context;
    public SqlSugarScope _sqlSugar;
    public SqlSugarScopeProvider _sqlSugarScopeProvider;
    public BaseRepository(IHttpContextAccessor context, SqlSugarScope sqlSugar)
    {
        _context = context;
        _sqlSugar = sqlSugar;
        _sqlSugarScopeProvider = sqlSugar.GetConnectionScopeWithAttr<TEntity>();
    }

    #region 事务操作
    public void BeginTran()
    {
        _sqlSugar.BeginTran();
    }
    public void CommitTran()
    {
        _sqlSugar.CommitTran();
    }
    public void RollbackTran()
    {
        _sqlSugar.RollbackTran();
    }

    #endregion

    #region 多租户
    /// <summary>
    /// 变更数据库
    /// </summary>
    /// <param name="db"></param>
    public void ChangeDataBase(DBEnum db)
    {
        _sqlSugar.ChangeDatabase(db);
    }
    #endregion

    #region 原生Sql
    public virtual Task<int> ExecuteCommandAsync(string sql)
    {
        return _sqlSugarScopeProvider.Ado.ExecuteCommandAsync(sql);
    }
    public virtual Task<DataTable> GetDataTableAsync(string sql)
    {
        return _sqlSugarScopeProvider.Ado.GetDataTableAsync(sql);
    }
    public virtual Task<object> GetScalarAsync(string sql)
    {
        return _sqlSugarScopeProvider.Ado.GetScalarAsync(sql);
    }
    public virtual Task<List<T>> SqlQueryAsync<T>(string sql)
    {
        return _sqlSugarScopeProvider.Ado.SqlQueryAsync<T>(sql);
    }
    public virtual ISugarQueryable<T> SqlQueryable<T>(string sql) where T : class, new()
    {
        return _sqlSugarScopeProvider.SqlQueryable<T>(sql);
    }
    #endregion

    #region 库表管理
    public bool IsTableExist()
    {
        var table = typeof(TEntity).GetCustomAttribute<SugarTable>();
        return _sqlSugarScopeProvider.DbMaintenance.IsAnyTable(table.TableName, false);
    }
    public bool IsTableExist<T>()
    {
        var table = typeof(T).GetCustomAttribute<SugarTable>();
        return _sqlSugar.GetConnectionScopeWithAttr<T>().DbMaintenance.IsAnyTable(table.TableName, false);
    }
    public bool CreateDataBase()
    {
        return _sqlSugarScopeProvider.DbMaintenance.CreateDatabase();
    }
    public bool CreateDataBase<T>()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().DbMaintenance.CreateDatabase();
    }
    public bool CopyTable(string newname)
    {
        var table = typeof(TEntity).GetCustomAttribute<SugarTable>();
        if (!_sqlSugarScopeProvider.DbMaintenance.IsAnyTable(newname, false))
        {
            return _sqlSugarScopeProvider.DbMaintenance.BackupTable(table.TableName, newname, 0);
        }
        return false;
    }
    public bool CopyTable<T>(string newname)
    {
        var table = typeof(T).GetCustomAttribute<SugarTable>();
        if (!_sqlSugar.GetConnectionScopeWithAttr<T>().DbMaintenance.IsAnyTable(newname, false))
        {
            return _sqlSugar.GetConnectionScopeWithAttr<T>().DbMaintenance.BackupTable(table.TableName, newname, 0);
        }
        return false;
    }
    public bool TruncateTable()
    {
        var table = typeof(TEntity).GetCustomAttribute<SugarTable>();
        return _sqlSugarScopeProvider.DbMaintenance.TruncateTable(table.TableName);
    }
    public bool TruncateTable<T>()
    {
        var table = typeof(T).GetCustomAttribute<SugarTable>();
        return _sqlSugar.GetConnectionScopeWithAttr<T>().DbMaintenance.TruncateTable(table.TableName);
    }
    public void CreateTable<T>()
    {
        _sqlSugar.GetConnectionScopeWithAttr<T>().CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(typeof(T));
    }
    public void CreateTable(Type[] entityTypes)
    {
        foreach (var entity in entityTypes)
        {
            var attr = entity.GetCustomAttribute<TenantAttribute>();
            if (attr != null)
            {
                _sqlSugar.ChangeDatabase(attr.configId);
            }
            else
            {
                _sqlSugar.ChangeDatabase(DBEnum.默认数据库);
            }
            _sqlSugar.CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(entity);
        }
    }
    #endregion

    #region CRUD
    public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().AnyAsync(exp);
    }
    public virtual ISugarQueryable<TEntity> Query(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp);
    }
    public virtual ISugarQueryable<TEntity> Query()
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().Where(a => !a.IsDeleted);
    }
    public virtual ISugarQueryable<TDto> QueryDto(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp).Select<TDto>();
    }
    public virtual ISugarQueryable<TDto> QueryDto()
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Select<TDto>();
    }
    public virtual Task<TDto> GetDtoAsync(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp).Select<TDto>().FirstAsync();
    }
    public virtual Task<int> AddAsync(TEntity entity)
    {
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return _sqlSugarScopeProvider.Insertable(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> AddAsync(List<TEntity> entities)
    {
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return _sqlSugarScopeProvider.Insertable(entities).ExecuteCommandAsync();
    }
    public virtual Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().FirstAsync(exp);
    }
    public virtual Task<TEntity> GetAsync(string id)
    {
        return _sqlSugarScopeProvider.Queryable<TEntity>().InSingleAsync(id);
    }
    public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> wherexp)
    {
        return _sqlSugarScopeProvider.Deleteable<TEntity>().Where(wherexp).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync(TEntity entity)
    {
        return _sqlSugarScopeProvider.Deleteable<TEntity>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync(List<TEntity> entities)
    {
        return _sqlSugarScopeProvider.Deleteable<TEntity>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync(Expression<Func<TEntity, bool>> wherexp, Expression<Func<TEntity, TEntity>> upexp)
    {
        return _sqlSugarScopeProvider.Updateable<TEntity>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync(TEntity entity)
    {
        return _sqlSugarScopeProvider.Updateable<TEntity>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync(List<TEntity> entities)
    {
        return _sqlSugarScopeProvider.Updateable<TEntity>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync(string id)
    {
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return _sqlSugarScopeProvider.Updateable<TEntity>().Where(a => a.Id.Equals(id)).SetColumns(a => new TEntity()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = userid
        }).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync(Expression<Func<TEntity, bool>> wherexp)
    {
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return _sqlSugarScopeProvider.Updateable<TEntity>().Where(wherexp).SetColumns(a => new TEntity()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = userid
        }).ExecuteCommandAsync();
    }
    #endregion

    #region 泛型CRUD
    public virtual Task<bool> AnyAsync<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().AnyAsync(exp);
    }
    public virtual ISugarQueryable<T> Query<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().Where(a => !a.IsDeleted).Where(exp);
    }
    public virtual ISugarQueryable<T> Query<T>() where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().Where(a => !a.IsDeleted);
    }
    public virtual Task<Dto> GetDtoAsync<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>().FirstAsync();
    }
    public virtual ISugarQueryable<Dto> QueryDto<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>();
    }
    public virtual ISugarQueryable<Dto> QueryDto<T, Dto>() where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().Where(a => !a.IsDeleted).Select<Dto>();
    }
    public virtual Task<int> AddAsync<T>(T entity) where T : EntityBase, new()
    {
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Insertable(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> AddAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Insertable(entities).ExecuteCommandAsync();
    }
    public virtual Task<T> GetAsync<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().Where(exp).FirstAsync();
    }
    public virtual Task<T> GetAsync<T>(string id) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Queryable<T>().InSingleAsync(id);
    }
    public virtual Task<int> UpdateAsync<T>(Expression<Func<T, bool>> wherexp, Expression<Func<T, T>> upexp) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Updateable<T>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync<T>(T entity) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Updateable<T>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Updateable<T>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Deleteable<T>().Where(wherexp).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(T entity) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Deleteable<T>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Deleteable<T>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync<T>(string id) where T : EntityBase, new()
    {
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Updateable<T>().Where(a => a.Id.Equals(id)).SetColumns(a => new T()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = userid
        }).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync<T>(Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
    {
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Updateable<T>().Where(wherexp).SetColumns(a => new T()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = userid
        }).ExecuteCommandAsync();
    }
    #endregion

    #region 自动分表
    public virtual Task<int> AddSplitTableAsync(TEntity entity)
    {
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return _sqlSugarScopeProvider.Insertable(entity).SplitTable().ExecuteCommandAsync();
    }
    public virtual Task<int> AddSplitTableAsync(List<TEntity> entities)
    {
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return _sqlSugarScopeProvider.Insertable(entities).SplitTable().ExecuteCommandAsync();
    }
    public virtual string GetTableName(DateTime datetime)
    {
        return _sqlSugarScopeProvider.SplitHelper<TEntity>().GetTableName(datetime);
    }
    public virtual Task<int> AddSplitTableAsync<T>(T entity) where T : EntityBase, new()
    {
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Insertable(entity).SplitTable().ExecuteCommandAsync();
    }
    public virtual Task<int> AddSplitTableAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Insertable(entities).SplitTable().ExecuteCommandAsync();
    }
    public virtual string GetTableName<T>(DateTime datetime) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().SplitHelper<T>().GetTableName(datetime);
    }
    #endregion

    #region 大数据写入
    public virtual Task<int> AddBulkAsync(List<TEntity> entities)
    {
        return _sqlSugarScopeProvider.Fastest<TEntity>().BulkCopyAsync(entities);
    }
    public virtual Task<int> UpdateBulkAsync(List<TEntity> entities)
    {
        return _sqlSugarScopeProvider.Fastest<TEntity>().BulkUpdateAsync(entities);
    }
    public virtual Task<int> AddBulkAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Fastest<T>().BulkCopyAsync(entities);
    }
    public virtual Task<int> UpdateBulkAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Fastest<T>().BulkUpdateAsync(entities);
    }
    public virtual Task<int> AddSplitTableBulkAsync(List<TEntity> entities)
    {
        return _sqlSugarScopeProvider.Fastest<TEntity>().SplitTable().BulkCopyAsync(entities);
    }
    public virtual Task<int> AddSplitTableBulkAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        return _sqlSugar.GetConnectionScopeWithAttr<T>().Fastest<T>().SplitTable().BulkCopyAsync(entities);
    }
    #endregion
}
