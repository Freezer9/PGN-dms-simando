namespace Simando.Domain.Survey;

// Orientasi Pasar — repeating, up to 4 rows + note. Same shape as
// SurveyRawMaterial; CountryId means destination country here rather than
// origin. docs/design/data-model.md#survey--stage-4-kk0-header.
public sealed class SurveyMarket
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
