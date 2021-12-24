using Microsoft.AspNetCore.Mvc;
using NET6.Domain.Dtos;
using NET6.Domain.Entities;
using NET6.Infrastructure.Repositories;
using NET6.Infrastructure.Tools;
using SqlSugar;
using System.Security.Claims;

namespace NET6.Api.Controllers
{
    /// <summary>
    /// 收货地址相关
    /// </summary>
    [Route("address")]
    public class AddressController : BaseController
    {
        readonly AddressRepository _addressRep;
        public AddressController(AddressRepository addressRep)
        {
            _addressRep = addressRep;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="size">每页条数</param>
        /// <returns></returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<AddressDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAsync(int page = 1, int size = 15)
        {
            var userid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var query = _addressRep.QueryDto(a => !a.IsDeleted);
            RefAsync<int> count = 0;
            var list = await query.OrderBy(a => a.IsDefault).ToPageListAsync(page, size, count);
            return Ok(JsonView(list, count));
        }

        /// <summary>
        /// 默认地址
        /// </summary>
        /// <returns></returns>
        [HttpGet("default")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> DefaultAsync()
        {
            var userid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var model = await _addressRep.GetDtoAsync(a => !a.IsDeleted && a.IsDefault && a.UserId == userid);
            if (model == null) return Ok(JsonView("未找到默认地址"));
            return Ok(JsonView(model));
        }

        /// <summary>
        /// 添加修改
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addoredit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddOrEditAsync(AddressDto dto)
        {
            var userid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (dto.Id.IsNull())
            {
                var result = await _addressRep.AddAsync(new Address
                {
                    UserId = userid,
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Province = dto.Province,
                    City = dto.City,
                    Area = dto.Area,
                    Detail = dto.Detail,
                    IsDefault = dto.IsDefault
                });
                if (result > 0) return Ok(JsonView(true));
                return Ok(JsonView("添加失败"));
            }
            else
            {
                var result = await _addressRep.UpdateAsync(a => a.Id == dto.Id, a => new Address
                {
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Province = dto.Province,
                    City = dto.City,
                    Area = dto.Area,
                    Detail = dto.Detail,
                    IsDefault = dto.IsDefault
                });
                if (result) return Ok(JsonView(true));
                return Ok(JsonView("修改失败"));
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("del")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DelAsync([FromBody] DeleteDto dto)
        {
            var result = await _addressRep.SoftDeleteAsync(a => a.Id == dto.Id);
            if (result) return Ok(JsonView(true));
            return Ok(JsonView("删除失败"));
        }
    }
}
