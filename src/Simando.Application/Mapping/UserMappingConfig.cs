using Mapster;
using Simando.Application.Security;
using Simando.Domain.Security;

namespace Simando.Application.Mapping;

public sealed class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RoleAssignment, RoleAssignmentDisplay>()
            .Map(dest => dest.Role, src => src.Role)
            .Map(dest => dest.ScopeLabel, src => src.RegionId.HasValue ? "Regional" : (src.AreaId.HasValue ? "Area" : "Nasional"));
    }
}
