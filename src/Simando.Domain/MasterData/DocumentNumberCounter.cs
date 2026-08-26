namespace Simando.Domain.MasterData;

// A locked counter row per document type/scope/period — official
// correspondence numbers (KK0, Nota Dinas) can't have gaps the way the
// company Nomor's lock-free sequence can. No surrogate id: the composite
// key below is the natural key. docs/domain/master-data.md §9.
public sealed class DocumentNumberCounter
{
    public required DocumentNumberType DocumentType { get; init; }

    // area_id or region_id, depending on DocumentType's counter scope.
    public required string ScopeKey { get; init; }

    // '2026' — resets yearly.
    public required string PeriodKey { get; init; }

    public required int NextSeq { get; set; }
}
