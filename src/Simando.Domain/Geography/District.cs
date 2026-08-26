using Simando.Domain.Common;

namespace Simando.Domain.Geography;

// Kecamatan.
public sealed class District : AuditableEntity
{
    public required Guid RegencyId { get; init; }
    public required string BpsCode { get; set; }
    public required string Name { get; set; }
}
