using Simando.Domain.Common;

namespace Simando.Domain.Geography;

public sealed class Province : AuditableEntity
{
    public required string BpsCode { get; set; }
    public required string Name { get; set; }
}
