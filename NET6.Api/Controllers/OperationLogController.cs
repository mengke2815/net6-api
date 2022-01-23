﻿using Microsoft.AspNetCore.Mvc;
using NET6.Api.Attributes;
using NET6.Domain.ViewModels;
using NET6.Infrastructure.Repositories;
using NET6.Infrastructure.Tools;
using SqlSugar;

namespace NET6.Api.Controllers
{
    /// <summary>
    /// 操作日志相关
    /// </summary>
    [Route("operationlog")]
    public class OperationLogController : BaseController
    {
        readonly OperationLogRepository _operationlogRep;
        public OperationLogController(OperationLogRepository operationlogRep)
        {
            _operationlogRep = operationlogRep;
        }

        /// <summary>
        /// 列表（只查询当月）
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page">当前页码</param>
        /// <param name="size">每页条数</param>
        /// <returns></returns>
        [NoLog]
        [HttpGet]
        [ProducesResponseType(typeof(List<OperationLogView>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAsync(string? keyword, int page = 1, int size = 15)
        {
            var tablename = _operationlogRep.GetTableName(DateTime.Now);
            var query = _operationlogRep.Query(a => !a.IsDeleted);
            if (keyword.NotNull())
            {
                query.Where(a => a.Params.Contains(keyword) || a.Result.Contains(keyword));
            }
            RefAsync<int> count = 0;
            var list = await query.SplitTable(a => a.InTableNames(tablename)).OrderBy(a => a.CreateTime, OrderByType.Desc).ToPageListAsync(page, size, count);
            return Ok(JsonView(list, count));
        }
    }
}
