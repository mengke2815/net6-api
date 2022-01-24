using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NET6.Domain.Dtos;
using NET6.Domain.Enums;
using NET6.Domain.ViewModels;
using NET6.Infrastructure.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NET6.Api.Controllers
{
    /// <summary>
    /// 鉴权相关
    /// </summary>
    public class AuthController : BaseController
    {
        readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _config = config;
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
            return Ok(JsonView(view));
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
                if (refreshtoken == null) return Ok(JsonView("未找到该刷新令牌"));
                if (refreshtoken != dto.RefreshToken) return Ok(JsonView("刷新令牌不正确"));
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
                return Ok(JsonView(view));
                #endregion
            }
            catch (Exception)
            {
                return Ok(JsonView("请求令牌解析失败"));
            }
        }
    }
}
