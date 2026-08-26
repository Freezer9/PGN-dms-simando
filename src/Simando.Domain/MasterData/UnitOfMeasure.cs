using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// One table for all eight unit sets — docs/domain/master-data.md §7.
public sealed class UnitOfMeasure : AuditableEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required UnitDimension Dimension { get; set; }
}
