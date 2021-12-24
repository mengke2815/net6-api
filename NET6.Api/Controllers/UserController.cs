using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace NET6.Api.Controllers
{
    /// <summary>
    /// 用户相关
    /// </summary>
    public class UserController : ControllerBase
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        public IActionResult ListAsync()
        {
            Log.Error("哈哈哈");
            return Ok(new { key = "1", Name = "李四" });
        }
    }
}
