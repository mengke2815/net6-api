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
    protected virtual JsonView JsonView(object obj)
    {
        return new JsonView { Code = StatusCodes.Status200OK, Msg = "操作成功", Data = obj };
    }
    protected virtual JsonView JsonView(object obj, int count)
    {
        return new JsonView { Code = StatusCodes.Status200OK, Msg = "操作成功", Data = obj, Count = count };
    }
    protected virtual JsonView JsonView(string msg)
    {
        return new JsonView { Code = StatusCodes.Status400BadRequest, Msg = msg };
    }
    protected virtual JsonView JsonView(bool s)
    {
        if (s)
        {
            return new JsonView { Code = StatusCodes.Status200OK, Msg = "操作成功" };
        }
        else
        {
            return new JsonView { Code = StatusCodes.Status400BadRequest, Msg = "操作失败" };
        }
    }
    protected virtual JsonView JsonView(bool s, string msg)
    {
        if (s)
        {
            return new JsonView { Code = StatusCodes.Status200OK, Msg = msg };
        }
        else
        {
            return new JsonView { Code = StatusCodes.Status400BadRequest, Msg = msg };
        }
    }
}
