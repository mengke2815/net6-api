using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NET6.Domain.Dtos;
using NET6.Domain.ViewModels;
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
            //校验用户信息
            var view = new LoginView
            {
                Expires = DateTime.Now.AddDays(30)
            };
            //用户信息
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "net6api.com",
                audience: "net6api.com",
                claims: claims,
                expires: view.Expires,
                signingCredentials: creds);
            view.Token = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(JsonView(view));
        }
    }
}
