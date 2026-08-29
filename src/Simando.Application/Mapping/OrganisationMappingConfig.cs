using Mapster;
using Simando.Domain.Organisation;

namespace Simando.Application.Mapping;

public sealed record RegionDto(Guid Id, string Code, string Name, bool Active);
public sealed record AreaDto(Guid Id, Guid RegionId, string Code, string Name, bool Active);

public sealed class OrganisationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Region, RegionDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Active, src => src.Active);

        config.NewConfig<Area, AreaDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.RegionId, src => src.RegionId)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Active, src => src.Active);
    }
}
