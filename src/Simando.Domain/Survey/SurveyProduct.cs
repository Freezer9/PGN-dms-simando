namespace Simando.Domain.Survey;

// Produk Utama — repeating, up to 4 rows + note. Satuan is fixed to
// "Kaps/Tahun" on every row (never varies), so it isn't a stored column.
// docs/design/data-model.md#survey--stage-4-kk0-header.
public sealed class SurveyProduct
{
    public required Guid Id { get; init; }
    public required Guid CompanyId { get; init; }

    public required string Produk { get; set; }
    public decimal? Kapasitas { get; set; }
    public decimal? HargaProduk { get; set; }
    public string? Catatan { get; set; }
    public required short SortOrder { get; set; }
}
