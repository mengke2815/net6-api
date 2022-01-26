using Microsoft.AspNetCore.Mvc;
using NET6.Domain.Dtos;
using NET6.Domain.Entities;
using NET6.Domain.ViewModels;
using NET6.Infrastructure.Repositories;
using NET6.Infrastructure.Tools;
using SqlSugar;

namespace NET6.Api.Controllers
{
    /// <summary>
    /// 地址相关
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
        /// 默认地址
        /// </summary>
        /// <returns></returns>
        [HttpGet("default")]
        [ProducesResponseType(typeof(AddressView), StatusCodes.Status200OK)]
        public async Task<IActionResult> DefaultAsync()
        {
            var model = await _addressRep.GetDtoAsync(a => a.IsDefault && a.UserId == CurrentUserId);
            if (model == null) return Ok(JsonView("未找到默认地址"));
            return Ok(JsonView(model));
        }

        /// <summary>
        /// 单个
        /// </summary>
        /// <param name="id">编号</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AddressView), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync(string id)
        {
            var model = await _addressRep.GetDtoAsync(a => a.Id == id);
            if (model == null) return Ok(JsonView("未找到数据"));
            return Ok(JsonView(model));
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page">当前页码</param>
        /// <param name="size">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<AddressView>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAsync(string keyword, int page = 1, int size = 15)
        {
            var query = _addressRep.QueryDto();
            if (keyword.NotNull())
            {
                query.Where(a => a.Detail.Contains(keyword));
            }
            RefAsync<int> count = 0;
            var list = await query.OrderBy(a => a.IsDefault).ToPageListAsync(page, size, count);
            return Ok(JsonView(list, count));
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddAsync(AddressDto dto)
        {
            try
            {
                //开启事务
                _addressRep.BeginTran();
                var result = await _addressRep.AddAsync(new Address
                {
                    UserId = CurrentUserId,
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Province = dto.Province,
                    City = dto.City,
                    Area = dto.Area,
                    Detail = dto.Detail,
                    IsDefault = dto.IsDefault
                });
                _addressRep.CommitTran();
                if (result > 0) return Ok(JsonView(true));
                return Ok(JsonView(false));
            }
            catch (Exception e)
            {
                _addressRep.RollbackTran();
                Logs("添加异常：" + e.Message);
                return Ok(JsonView("添加异常"));
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="id">编号</param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditAsync(string id, AddressDto dto)
        {
            try
            {
                //开启事务
                _addressRep.BeginTran();
                var result = await _addressRep.UpdateAsync(a => a.Id == id, a => new Address
                {
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Province = dto.Province,
                    City = dto.City,
                    Area = dto.Area,
                    Detail = dto.Detail,
                    IsDefault = dto.IsDefault
                });
                _addressRep.CommitTran();
                if (result > 0) return Ok(JsonView(true));
                return Ok(JsonView(false));
            }
            catch (Exception e)
            {
                _addressRep.RollbackTran();
                Logs("修改异常：" + e.Message);
                return Ok(JsonView("修改异常"));
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">编号</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                //开启事务操作资源
                _addressRep.BeginTran();
                var result = await _addressRep.SoftDeleteAsync(a => a.Id == id);
                _addressRep.CommitTran();
                if (result > 0) return Ok(JsonView(true));
                return Ok(JsonView(false));
            }
            catch (Exception e)
            {
                _addressRep.RollbackTran();
                Logs("删除异常：" + e.Message);
                return Ok(JsonView("删除异常"));
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="page">当前页码</param>
        /// <param name="size">每页条数</param>
        /// <returns></returns>
        [HttpGet("export")]
        [ProducesResponseType(typeof(List<AddressView>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportAsync(int page = 1, int size = 15)
        {
            var query = _addressRep.QueryDto();
            RefAsync<int> count = 0;
            var list = await query.OrderBy(a => a.IsDefault).ToPageListAsync(page, size, count);
            var columns = new Dictionary<string, string>
            {
                { "Name", "收件人名称" },
                { "Phone", "收件人电话" },
                { "Province", "省份" },
                { "City", "城市" },
                { "Detail", "详细地址" }
            };
            var title = $"列表导出({DateTime.Now:yyyyMMddHHmmss})";
            var outColumn = new List<string>();
            var fs = ExcelExportImportHelper.GetByteToExportExcel(list, columns, outColumn, "列表导出", title, false);
            return File(fs, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", title + ".xlsx");
        }
    }
}
