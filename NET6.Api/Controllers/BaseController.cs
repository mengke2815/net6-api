namespace NET6.Api.Controllers;

/// <summary>
/// 控制器基类
/// </summary>
[Authorize]
[ApiController]
public class BaseController : ControllerBase
{
    protected virtual string CurrentUserId => HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    protected virtual void Logs(string str)
    {
        Log.Error(str);
    }
    #region 统一返回结构
    protected virtual IActionResult JsonView(object obj)
    {
        return Ok(new JsonView { Code = StatusCodes.Status200OK, Msg = "操作成功", Data = obj });
    }
    protected virtual IActionResult JsonView(object obj, int count)
    {
        return Ok(new JsonView { Code = StatusCodes.Status200OK, Msg = "操作成功", Data = obj, Count = count });
    }
    protected virtual IActionResult JsonView(string msg)
    {
        return Ok(new JsonView { Code = StatusCodes.Status400BadRequest, Msg = msg });
    }
    protected virtual IActionResult JsonView(bool s)
    {
        if (s)
        {
            return Ok(new JsonView { Code = StatusCodes.Status200OK, Msg = "操作成功" });
        }
        else
        {
            return Ok(new JsonView { Code = StatusCodes.Status400BadRequest, Msg = "操作失败" });
        }
    }
    protected virtual IActionResult JsonView(bool s, string msg)
    {
        if (s)
        {
            return Ok(new JsonView { Code = StatusCodes.Status200OK, Msg = msg });
        }
        else
        {
            return Ok(new JsonView { Code = StatusCodes.Status400BadRequest, Msg = msg });
        }
    }
    #endregion
}
