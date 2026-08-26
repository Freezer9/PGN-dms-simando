namespace Simando.Domain.MasterData;

// Which units populate which <MeasureInput Set="..."> dropdown, and in what
// order. docs/domain/master-data.md §7.
public sealed class UnitSetMember
{
    public required Guid Id { get; init; }
    public required UnitSet SetCode { get; init; }
    public required Guid UnitId { get; init; }
    public required int SortOrder { get; set; }
}
