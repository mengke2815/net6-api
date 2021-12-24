using NET6.Domain.Dtos;
using NET6.Domain.Entities;
using SqlSugar;

namespace NET6.Infrastructure.Repositories
{
    public class AddressRepository : BaseRepository<Address, AddressDto>
    {
        public AddressRepository(SqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
