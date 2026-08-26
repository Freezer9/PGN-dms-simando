namespace Simando.Application.Directory;

public sealed record SurveyProductDetail(
    Guid Id,
    string Produk,
    decimal? Kapasitas,
    decimal? HargaProduk,
    string? Catatan,
    short SortOrder);

public sealed record SaveSurveyProductRequest(
    string Produk,
    decimal? Kapasitas,
    decimal? HargaProduk,
    string? Catatan);
