﻿namespace NET6.Api.Controllers;

/// <summary>
/// 鉴权相关
/// </summary>
public class AuthController : BaseController
{
    readonly IConfiguration _config;
    readonly IEventPublisher _eventPublisher;
    public AuthController(IConfiguration config, IEventPublisher eventPublisher)
    {
        _config = config;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginView), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync(LoginDto dto)
    {
        #region 事件总线发布
        await _eventPublisher.PublishAsync(new ChannelEventSource("Login", "这里是用户登录消息"));
        #endregion
        #region 校验用户信息
        var userid = "123";
        var username = "admin";
        #endregion
        #region 签发JWT
        //生成一个刷新令牌
        var refreshtoken = CommonFun.GUID;
        CacheHelper.Set($"{CacheEnum.刷新令牌}_{userid}", refreshtoken, TimeSpan.FromDays(30));
        var view = new LoginView
        {
            Expires = DateTime.Now.AddDays(7),
            RefreshToken = refreshtoken
        };
        //用户信息
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, userid),
            new Claim(ClaimTypes.Name, username)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSecurityKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "net6api.com",
            audience: "net6api.com",
            claims: claims,
            expires: view.Expires,
            signingCredentials: creds);
        view.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return JsonView(view);
        #endregion
    }

    /// <summary>
    /// 刷新访问令牌
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginView), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshAsync(RefreshTokenDto dto)
    {
        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(dto.AccessToken);
            var userid = jwtToken.Claims.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = jwtToken.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name)?.Value;
            var refreshtoken = CacheHelper.Get<string>($"{CacheEnum.刷新令牌}_{userid}");
            if (refreshtoken == null) return JsonView("未找到该刷新令牌");
            if (refreshtoken != dto.RefreshToken) return JsonView("刷新令牌不正确");
            #region 签发JWT
            //生成一个新的刷新令牌
            refreshtoken = CommonFun.GUID;
            CacheHelper.Set($"{CacheEnum.刷新令牌}_{userid}", refreshtoken, TimeSpan.FromDays(30));
            var view = new LoginView
            {
                Expires = DateTime.Now.AddDays(7),
                RefreshToken = refreshtoken
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "net6api.com",
                audience: "net6api.com",
                claims: jwtToken.Claims,
                expires: view.Expires,
                signingCredentials: creds);
            view.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return JsonView(view);
            #endregion
        }
        catch (Exception)
        {
            return JsonView("访问令牌解析失败");
        }
    }
}
