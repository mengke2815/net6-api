namespace NET6.Infrastructure.Repositories;

/// <summary>
/// 地址仓储
/// </summary>
public class AddressRepository : BaseRepository<Address, AddressView>
{
    public AddressRepository(IHttpContextAccessor context, IMapper mapper, SqlSugarScope sqlSugar) : base(context, sqlSugar)
    {

    }
}
