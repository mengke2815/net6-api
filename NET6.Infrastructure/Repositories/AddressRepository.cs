using NET6.Domain.Entities;
using NET6.Domain.ViewModels;
using SqlSugar;

namespace NET6.Infrastructure.Repositories
{
    public class AddressRepository : BaseRepository<Address, AddressView>
    {
        public AddressRepository(SqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
