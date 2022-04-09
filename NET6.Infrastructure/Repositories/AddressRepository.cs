using Microsoft.AspNetCore.Http;
using NET6.Domain.Entities;
using NET6.Domain.ViewModels;
using SqlSugar;

namespace NET6.Infrastructure.Repositories
{
    /// <summary>
    /// 地址仓储
    /// </summary>
    public class AddressRepository : BaseRepository<Address, AddressView>
    {
        public AddressRepository(IHttpContextAccessor context, SqlSugarScope sqlSugar) : base(context, sqlSugar)
        {

        }
    }
}
