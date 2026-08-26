namespace Simando.Domain.Survey;

// Bahan Baku — repeating, up to 4 rows + note. Periode is fixed to
// "per_bulan" on every row (never varies), so it isn't a stored column.
// SatuanUnitId is scoped to UnitSet.RawMaterial (%, Ton, KL, m2).
// docs/design/data-model.md#survey--stage-4-kk0-header.
public sealed class SurveyRawMaterial
{
    public required Guid Id { get; init; }
    public required Guid CompanyId { get; init; }

    public string? Bahan { get; set; }
    public Asal? Asal { get; set; }
    public Guid? CountryId { get; set; }
    public decimal? Volume { get; set; }
    public Guid? SatuanUnitId { get; set; }
    public required short SortOrder { get; set; }
}
