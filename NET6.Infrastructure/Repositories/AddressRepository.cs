namespace NET6.Infrastructure.Repositories;

/// <summary>
/// 地址仓储
/// </summary>
public class AddressRepository : BaseRepository<Address, AddressView>
{
    public AddressRepository(IMapper mapper, SqlSugarScope sqlSugar) : base(sqlSugar)
    {

    }
}
