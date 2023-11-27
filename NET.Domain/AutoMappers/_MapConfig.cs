namespace NET.Domain.AutoMappers;

/// <summary>
/// 自动映射
/// </summary>
public class _MapConfig : Profile
{
    public _MapConfig()
    {
        CreateMap<AddressDto, Address>();
        CreateMap<Address, AddressView>();
        CreateMap<OperationLog, OperationLogView>();
    }
}
