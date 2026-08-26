using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// Optional grouping alongside the mandatory free-text comment on
// Revisi/Tolak, so the NOL-outcomes report can group rejection reasons.
// Ships empty — do not invent categories. docs/domain/master-data.md §10.
public sealed class ReasonCategory : AuditableEntity
{
    public required string Name { get; set; }
}
