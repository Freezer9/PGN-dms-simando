using Simando.Domain.Common;

namespace Simando.Domain.Geography;

// Covers both Kota and Kabupaten — see RegencyType.
public sealed class Regency : AuditableEntity
{
    public required Guid ProvinceId { get; init; }
    public required string BpsCode { get; set; }
    public required RegencyType Type { get; set; }
    public required string Name { get; set; }
}
