namespace NET7.Api.Controllers;

/// <summary>
/// 操作日志相关
/// </summary>
[Route("operationlog")]
public class OperationLogController : BaseController
{
    readonly IMapper _mapper;
    readonly OperationLogRepository _operationlogRep;
    public OperationLogController(IMapper mapper, OperationLogRepository operationlogRep)
    {
        _mapper = mapper;
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
    public async Task<IActionResult> ListAsync(string keyword, int page = 1, int size = 15)
    {
        var tablename = _operationlogRep.GetTableName(DateTime.Now);
        var query = _operationlogRep.Query();
        if (keyword.NotNull())
        {
            query.Where(a => a.Params.Contains(keyword) || a.Result.Contains(keyword));
        }
        RefAsync<int> count = 0;
        var list = await query.SplitTable(a => a.InTableNames(tablename)).OrderBy(a => a.CreateTime, OrderByType.Desc).ToPageListAsync(page, size, count);
        var result = _mapper.Map<List<OperationLogView>>(list);
        return JsonView(result, count);
    }
}
