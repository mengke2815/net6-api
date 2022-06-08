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
    public SqlSugarScopeProvider _sqlSugarProvider;
    public BaseRepository(IHttpContextAccessor context, SqlSugarScope sqlSugar)
    {
        _context = context;
        _sqlSugar = sqlSugar;
        _sqlSugarProvider = sqlSugar.GetConnectionScopeWithAttr<TEntity>();
    }

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
        return _sqlSugar.Ado.ExecuteCommandAsync(sql);
    }
    public virtual Task<DataTable> GetDataTableAsync(string sql)
    {
        return _sqlSugar.Ado.GetDataTableAsync(sql);
    }
    public virtual Task<object> GetScalarAsync(string sql)
    {
        return _sqlSugar.Ado.GetScalarAsync(sql);
    }
    #endregion

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

    #region 库表管理
    public bool IsTableExist()
    {
        var table = typeof(TEntity).GetCustomAttribute<SugarTable>();
        return _sqlSugarProvider.DbMaintenance.IsAnyTable(table.TableName, false);
    }
    public bool IsTableExist<T>()
    {
        var tenant = typeof(T).GetCustomAttribute<TenantAttribute>();
        if (tenant != null)
        {
            _sqlSugar.ChangeDatabase(tenant.configId);
        }
        else
        {
            _sqlSugar.ChangeDatabase(DBEnum.默认数据库);
        }
        var table = typeof(T).GetCustomAttribute<SugarTable>();
        return _sqlSugar.DbMaintenance.IsAnyTable(table.TableName, false);
    }
    public bool CreateDataBase()
    {
        return _sqlSugarProvider.DbMaintenance.CreateDatabase();
    }
    public bool CreateDataBase<T>()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.DbMaintenance.CreateDatabase();
    }
    public bool CopyTable(string newname)
    {
        var table = typeof(TEntity).GetCustomAttribute<SugarTable>();
        if (!_sqlSugarProvider.DbMaintenance.IsAnyTable(newname, false))
        {
            return _sqlSugarProvider.DbMaintenance.BackupTable(table.TableName, newname, 0);
        }
        return false;
    }
    public bool CopyTable<T>(string newname)
    {
        var tenant = typeof(T).GetCustomAttribute<TenantAttribute>();
        if (tenant != null)
        {
            _sqlSugar.ChangeDatabase(tenant.configId);
        }
        else
        {
            _sqlSugar.ChangeDatabase(DBEnum.默认数据库);
        }
        var table = typeof(T).GetCustomAttribute<SugarTable>();
        if (!_sqlSugar.DbMaintenance.IsAnyTable(newname, false))
        {
            return _sqlSugar.DbMaintenance.BackupTable(table.TableName, newname, 0);
        }
        return false;
    }
    public bool TruncateTable()
    {
        var table = typeof(TEntity).GetCustomAttribute<SugarTable>();
        return _sqlSugarProvider.DbMaintenance.TruncateTable(table.TableName);
    }
    public bool TruncateTable<T>()
    {
        var tenant = typeof(T).GetCustomAttribute<TenantAttribute>();
        if (tenant != null)
        {
            _sqlSugar.ChangeDatabase(tenant.configId);
        }
        else
        {
            _sqlSugar.ChangeDatabase(DBEnum.默认数据库);
        }
        var table = typeof(T).GetCustomAttribute<SugarTable>();
        return _sqlSugar.DbMaintenance.TruncateTable(table.TableName);
    }
    public void CreateTable<T>()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        provider.CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(typeof(T));
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
        return _sqlSugarProvider.Queryable<TEntity>().AnyAsync(exp);
    }
    public virtual ISugarQueryable<TEntity> Query(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp);
    }
    public virtual ISugarQueryable<TEntity> Query()
    {
        return _sqlSugarProvider.Queryable<TEntity>().Where(a => !a.IsDeleted);
    }
    public virtual ISugarQueryable<TDto> QueryDto(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp).Select<TDto>();
    }
    public virtual ISugarQueryable<TDto> QueryDto()
    {
        return _sqlSugarProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Select<TDto>();
    }
    public virtual Task<TDto> GetDtoAsync(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarProvider.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp).Select<TDto>().FirstAsync();
    }
    public virtual Task<int> AddAsync(TEntity entity)
    {
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return _sqlSugarProvider.Insertable(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> AddAsync(List<TEntity> entities)
    {
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return _sqlSugarProvider.Insertable(entities).ExecuteCommandAsync();
    }
    public virtual Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> exp)
    {
        return _sqlSugarProvider.Queryable<TEntity>().FirstAsync(exp);
    }
    public virtual Task<TEntity> GetAsync(string id)
    {
        return _sqlSugarProvider.Queryable<TEntity>().InSingleAsync(id);
    }
    public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> wherexp)
    {
        return _sqlSugarProvider.Deleteable<TEntity>().Where(wherexp).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync(TEntity entity)
    {
        return _sqlSugarProvider.Deleteable<TEntity>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync(List<TEntity> entities)
    {
        return _sqlSugarProvider.Deleteable<TEntity>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync(Expression<Func<TEntity, bool>> wherexp, Expression<Func<TEntity, TEntity>> upexp)
    {
        return _sqlSugarProvider.Updateable<TEntity>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync(TEntity entity)
    {
        return _sqlSugarProvider.Updateable<TEntity>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync(List<TEntity> entities)
    {
        return _sqlSugarProvider.Updateable<TEntity>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync(string id)
    {
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return _sqlSugarProvider.Updateable<TEntity>().Where(a => a.Id.Equals(id)).SetColumns(a => new TEntity()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = userid
        }).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync(Expression<Func<TEntity, bool>> wherexp)
    {
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return _sqlSugarProvider.Updateable<TEntity>().Where(wherexp).SetColumns(a => new TEntity()
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
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().AnyAsync(exp);
    }
    public virtual ISugarQueryable<T> Query<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Where(exp);
    }
    public virtual ISugarQueryable<T> Query<T>() where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted);
    }
    public virtual Task<Dto> GetDtoAsync<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>().FirstAsync();
    }
    public virtual ISugarQueryable<Dto> QueryDto<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>();
    }
    public virtual ISugarQueryable<Dto> QueryDto<T, Dto>() where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Select<Dto>();
    }
    public virtual Task<int> AddAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return provider.Insertable(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> AddAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return provider.Insertable(entities).ExecuteCommandAsync();
    }
    public virtual Task<T> GetAsync<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().Where(exp).FirstAsync();
    }
    public virtual Task<T> GetAsync<T>(string id) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Queryable<T>().InSingleAsync(id);
    }
    public virtual Task<int> UpdateAsync<T>(Expression<Func<T, bool>> wherexp, Expression<Func<T, T>> upexp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Updateable<T>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Updateable<T>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Updateable<T>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Deleteable<T>().Where(wherexp).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Deleteable<T>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Deleteable<T>(entities).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync<T>(string id) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return provider.Updateable<T>().Where(a => a.Id.Equals(id)).SetColumns(a => new T()
        {
            IsDeleted = true,
            DeleteTime = DateTime.Now,
            DeleteUserId = userid
        }).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync<T>(Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        var userid = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return provider.Updateable<T>().Where(wherexp).SetColumns(a => new T()
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
        return _sqlSugarProvider.Insertable(entity).SplitTable().ExecuteCommandAsync();
    }
    public virtual Task<int> AddSplitTableAsync(List<TEntity> entities)
    {
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return _sqlSugarProvider.Insertable(entities).SplitTable().ExecuteCommandAsync();
    }
    public virtual string GetTableName(DateTime datetime)
    {
        return _sqlSugarProvider.SplitHelper<TEntity>().GetTableName(datetime);
    }
    public virtual Task<int> AddSplitTableAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return provider.Insertable(entity).SplitTable().ExecuteCommandAsync();
    }
    public virtual Task<int> AddSplitTableAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        foreach (var item in entities)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entities);
        return provider.Insertable(entities).SplitTable().ExecuteCommandAsync();
    }
    public virtual string GetTableName<T>(DateTime datetime) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.SplitHelper<T>().GetTableName(datetime);
    }
    #endregion

    #region 大数据写入
    public virtual Task<int> AddBulkAsync(List<TEntity> entities)
    {
        return _sqlSugarProvider.Fastest<TEntity>().BulkCopyAsync(entities);
    }
    public virtual Task<int> UpdateBulkAsync(List<TEntity> entities)
    {
        return _sqlSugarProvider.Fastest<TEntity>().BulkUpdateAsync(entities);
    }
    public virtual Task<int> AddBulkAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Fastest<T>().BulkCopyAsync(entities);
    }
    public virtual Task<int> UpdateBulkAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Fastest<T>().BulkUpdateAsync(entities);
    }
    public virtual Task<int> AddSplitTableBulkAsync(List<TEntity> entities)
    {
        return _sqlSugarProvider.Fastest<TEntity>().SplitTable().BulkCopyAsync(entities);
    }
    public virtual Task<int> AddSplitTableBulkAsync<T>(List<T> entities) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionScopeWithAttr<T>();
        return provider.Fastest<T>().SplitTable().BulkCopyAsync(entities);
    }
    #endregion
}
