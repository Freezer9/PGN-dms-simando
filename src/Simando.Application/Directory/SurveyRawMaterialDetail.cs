using Simando.Domain.Survey;

namespace Simando.Application.Directory;

public sealed record SurveyRawMaterialDetail(
    Guid Id,
    string? Bahan,
    Asal? Asal,
    Guid? CountryId,
    decimal? Volume,
    Guid? SatuanUnitId,
    short SortOrder);

public sealed record SaveSurveyRawMaterialRequest(
    string? Bahan,
    Asal? Asal,
    Guid? CountryId,
    decimal? Volume,
    Guid? SatuanUnitId);
