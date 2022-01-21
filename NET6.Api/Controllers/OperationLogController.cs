using Microsoft.AspNetCore.Mvc;
using NET6.Domain.ViewModels;
using NET6.Infrastructure.Repositories;
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
        /// 列表
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="size">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<OperationLogView>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAsync(int page = 1, int size = 15)
        {
            var query = _operationlogRep.QueryDto(a => !a.IsDeleted);
            RefAsync<int> count = 0;
            var list = await query.OrderBy(a => a.CreateTime, OrderByType.Desc).ToPageListAsync(page, size, count);
            return Ok(JsonView(list, count));
        }
    }
}
