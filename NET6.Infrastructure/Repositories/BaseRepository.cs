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
    public SqlSugarProvider _sqlSugarProvider;
    public BaseRepository(IHttpContextAccessor context, SqlSugarScope sqlSugar)
    {
        _context = context;
        _sqlSugar = sqlSugar;
        _sqlSugarProvider = sqlSugar.GetConnectionWithAttr<TEntity>();
    }

    #region 多租户
    /// <summary>
    /// 变更数据库
    /// </summary>
    /// <param name="db"></param>
    public void ChangeDataBase(DBEnum db)
    {
        _sqlSugarProvider.AsTenant().ChangeDatabase(db);
    }
    #endregion

    #region 原生Sql
    public virtual Task<int> ExecuteCommandAsync(string sql)
    {
        return _sqlSugarProvider.Ado.ExecuteCommandAsync(sql);
    }
    public virtual Task<DataTable> GetDataTableAsync(string sql)
    {
        return _sqlSugarProvider.Ado.GetDataTableAsync(sql);
    }
    public virtual Task<object> GetScalarAsync(string sql)
    {
        return _sqlSugarProvider.Ado.GetScalarAsync(sql);
    }
    #endregion

    #region 事务操作
    public void BeginTran()
    {
        _sqlSugarProvider.AsTenant().BeginTran();
    }
    public void CommitTran()
    {
        _sqlSugarProvider.AsTenant().CommitTran();
    }
    public void RollbackTran()
    {
        _sqlSugarProvider.AsTenant().RollbackTran();
    }
    #endregion

    #region 库表管理
    public bool IsTableExist(string tablename)
    {
        return _sqlSugarProvider.DbMaintenance.IsAnyTable(tablename, false);
    }
    public bool CreateDataBase()
    {
        return _sqlSugarProvider.DbMaintenance.CreateDatabase();
    }
    public bool CopyTable(string oldname, string newname)
    {
        if (!_sqlSugarProvider.DbMaintenance.IsAnyTable(newname, false))
        {
            return _sqlSugarProvider.DbMaintenance.BackupTable(oldname, newname, 0);
        }
        return false;
    }
    public bool TruncateTable(string tablename)
    {
        return _sqlSugarProvider.DbMaintenance.TruncateTable(tablename);
    }
    public void CreateTable(Type entityType)
    {
        _sqlSugarProvider.CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(entityType);
    }
    public void CreateTable(Type[] entityTypes)
    {
        _sqlSugarProvider.CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(entityTypes);
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
    public virtual Task<int> AddAsync(List<TEntity> entitys)
    {
        foreach (var item in entitys)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entitys);
        return _sqlSugarProvider.Insertable(entitys).ExecuteCommandAsync();
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
    public virtual Task<int> UpdateAsync(Expression<Func<TEntity, bool>> wherexp, Expression<Func<TEntity, TEntity>> upexp)
    {
        return _sqlSugarProvider.Updateable<TEntity>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync(TEntity entity)
    {
        return _sqlSugarProvider.Updateable<TEntity>(entity).ExecuteCommandAsync();
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
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().AnyAsync(exp);
    }
    public virtual ISugarQueryable<T> Query<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Where(exp);
    }
    public virtual ISugarQueryable<T> Query<T>() where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted);
    }
    public virtual Task<Dto> GetDtoAsync<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>().FirstAsync();
    }
    public virtual ISugarQueryable<Dto> QueryDto<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>();
    }
    public virtual ISugarQueryable<Dto> QueryDto<T, Dto>() where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().Where(a => !a.IsDeleted).Select<Dto>();
    }
    public virtual Task<int> AddAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return provider.Insertable(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> AddAsync<T>(List<T> entitys) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        foreach (var item in entitys)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entitys);
        return provider.Insertable(entitys).ExecuteCommandAsync();
    }
    public virtual Task<T> GetAsync<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().Where(exp).FirstAsync();
    }
    public virtual Task<T> GetAsync<T>(string id) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Queryable<T>().InSingleAsync(id);
    }
    public virtual Task<int> UpdateAsync<T>(Expression<Func<T, bool>> wherexp, Expression<Func<T, T>> upexp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Updateable<T>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
    }
    public virtual Task<int> UpdateAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Updateable<T>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Deleteable<T>().Where(wherexp).ExecuteCommandAsync();
    }
    public virtual Task<int> DeleteAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.Deleteable<T>(entity).ExecuteCommandAsync();
    }
    public virtual Task<int> SoftDeleteAsync<T>(string id) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
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
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
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
    public virtual Task<int> AddSplitTableAsync(List<TEntity> entitys)
    {
        foreach (var item in entitys)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entitys);
        return _sqlSugarProvider.Insertable(entitys).SplitTable().ExecuteCommandAsync();
    }
    public virtual string GetTableName(DateTime datetime)
    {
        return _sqlSugarProvider.SplitHelper<TEntity>().GetTableName(datetime);
    }
    public virtual Task<int> AddSplitTableAsync<T>(T entity) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        entity.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CommonFun.CoverNull(entity);
        return provider.Insertable(entity).SplitTable().ExecuteCommandAsync();
    }
    public virtual Task<int> AddSplitTableAsync<T>(List<T> entitys) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        foreach (var item in entitys)
        {
            item.CreateUserId = _context?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        CommonFun.CoverNull(entitys);
        return provider.Insertable(entitys).SplitTable().ExecuteCommandAsync();
    }
    public virtual string GetTableName<T>(DateTime datetime) where T : EntityBase, new()
    {
        var provider = _sqlSugar.GetConnectionWithAttr<T>();
        return provider.SplitHelper<T>().GetTableName(datetime);
    }
    #endregion
}
