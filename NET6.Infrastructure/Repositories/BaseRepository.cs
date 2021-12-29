using NET6.Domain.Entities;
using NET6.Domain.Enums;
using NET6.Infrastructure.Tools;
using SqlSugar;
using System.Data;
using System.Linq.Expressions;

namespace NET6.Infrastructure.Repositories
{
    public class BaseRepository<TEntity, TDto> where TEntity : EntityBase, new()
    {
        public SqlSugarClient _sqlSugar;
        public BaseRepository(SqlSugarClient sqlSugar)
        {
            _sqlSugar = sqlSugar;
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
        public bool CreateDataBase()
        {
            return _sqlSugar.DbMaintenance.CreateDatabase();
        }
        public bool CopyTable(string oldname, string newname)
        {
            if (!_sqlSugar.DbMaintenance.IsAnyTable(newname, false))
            {
                return _sqlSugar.DbMaintenance.BackupTable(oldname, newname, 0);
            }
            return false;
        }
        public bool TruncateTable(string tablename)
        {
            return _sqlSugar.DbMaintenance.TruncateTable(tablename);
        }
        public void CreateTable(Type entityType)
        {
            _sqlSugar.CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(entityType);
        }
        public void CreateTable(Type[] entityTypes)
        {
            _sqlSugar.CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(entityTypes);
        }
        #endregion

        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> exp)
        {
            return _sqlSugar.Queryable<TEntity>().AnyAsync(exp);
        }
        public virtual ISugarQueryable<TEntity> Query(Expression<Func<TEntity, bool>> exp)
        {
            return _sqlSugar.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp);
        }
        public virtual ISugarQueryable<TDto> QueryDto(Expression<Func<TEntity, bool>> exp)
        {
            return _sqlSugar.Queryable<TEntity>().Where(a => !a.IsDeleted).Where(exp).Select<TDto>();
        }
        public virtual Task<TDto> GetDtoAsync(Expression<Func<TEntity, bool>> exp)
        {
            return _sqlSugar.Queryable<TEntity>().Where(s => !s.IsDeleted).Where(exp).Select<TDto>().FirstAsync();
        }
        public virtual Task<int> AddAsync(TEntity entity)
        {
            CommonFun.CoverNull(entity);
            return _sqlSugar.Insertable(entity).ExecuteCommandAsync();
        }
        public virtual Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> exp)
        {
            return _sqlSugar.Queryable<TEntity>().FirstAsync(exp);
        }
        public virtual async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> wherexp)
        {
            var result = await _sqlSugar.Deleteable<TEntity>().Where(wherexp).ExecuteCommandAsync();
            return result > 0;
        }
        public virtual async Task<bool> UpdateAsync(Expression<Func<TEntity, bool>> wherexp, Expression<Func<TEntity, TEntity>> upexp)
        {
            var result = await _sqlSugar.Updateable<TEntity>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
            return result > 0;
        }
        public virtual async Task<bool> SoftDeleteAsync(string id)
        {
            var result = await _sqlSugar.Updateable<TEntity>().Where(a => a.Id.Equals(id)).SetColumns(a => new TEntity()
            {
                IsDeleted = true,
                DeleteTime = DateTime.Now
            }).ExecuteCommandAsync();
            return result > 0;
        }
        public virtual async Task<bool> SoftDeleteAsync(Expression<Func<TEntity, bool>> wherexp)
        {
            var result = await _sqlSugar.Updateable<TEntity>().Where(wherexp).SetColumns(a => new TEntity()
            {
                IsDeleted = true,
                DeleteTime = DateTime.Now
            }).ExecuteCommandAsync();
            return result > 0;
        }

        #region 泛型CRUD
        public virtual Task<bool> AnyAsync<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
        {
            return _sqlSugar.Queryable<T>().Where(s => !s.IsDeleted).AnyAsync(exp);
        }
        public virtual Task<Dto> GetDtoAsync<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
        {
            return _sqlSugar.Queryable<T>().Where(s => !s.IsDeleted).Where(exp).Select<Dto>().FirstAsync();
        }
        public virtual ISugarQueryable<Dto> QueryDto<T, Dto>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
        {
            return _sqlSugar.Queryable<T>().Where(a => !a.IsDeleted).Where(exp).Select<Dto>();
        }
        public virtual Task<int> AddAsync<T>(T entity) where T : EntityBase, new()
        {
            CommonFun.CoverNull(entity);
            return _sqlSugar.Insertable(entity).ExecuteCommandAsync();
        }
        public virtual ISugarQueryable<T> Query<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
        {
            return _sqlSugar.Queryable<T>().Where(s => !s.IsDeleted).Where(exp);
        }
        public virtual Task<T> GetAsync<T>(Expression<Func<T, bool>> exp) where T : EntityBase, new()
        {
            return _sqlSugar.Queryable<T>().Where(s => !s.IsDeleted).Where(exp).FirstAsync();
        }
        public virtual async Task<bool> UpdateAsync<T>(Expression<Func<T, bool>> wherexp, Expression<Func<T, T>> upexp) where T : EntityBase, new()
        {
            var result = await _sqlSugar.Updateable<T>().Where(wherexp).SetColumns(upexp).ExecuteCommandAsync();
            return result > 0;
        }
        public virtual async Task<bool> DeleteAsync<T>(Expression<Func<T, bool>> wherexp) where T : EntityBase, new()
        {
            var result = await _sqlSugar.Deleteable<T>().Where(wherexp).ExecuteCommandAsync();
            return result > 0;
        }
        public virtual async Task<bool> SoftDeleteAsync<T>(string id) where T : EntityBase, new()
        {
            var result = await _sqlSugar.Updateable<TEntity>().Where(a => a.Id.Equals(id)).SetColumns(a => new TEntity()
            {
                IsDeleted = true,
                DeleteTime = DateTime.Now
            }).ExecuteCommandAsync();
            return result > 0;
        }
        public virtual async Task<bool> SoftDeleteAsync<T>(Expression<Func<TEntity, bool>> wherexp) where T : EntityBase, new()
        {
            var result = await _sqlSugar.Updateable<TEntity>().Where(wherexp).SetColumns(a => new TEntity()
            {
                IsDeleted = true,
                DeleteTime = DateTime.Now
            }).ExecuteCommandAsync();
            return result > 0;
        }
        #endregion
    }
}
