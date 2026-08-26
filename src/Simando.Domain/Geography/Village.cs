using Simando.Domain.Common;

namespace Simando.Domain.Geography;

// Kelurahan/Desa — see VillageType.
public sealed class Village : AuditableEntity
{
    public required Guid DistrictId { get; init; }
    public required string BpsCode { get; set; }
    public required VillageType Type { get; set; }
    public required string Name { get; set; }
}
