using Simando.Domain.Survey;

namespace Simando.Application.Directory;

// Same shape as SurveyRawMaterialDetail — CountryId means destination here
// rather than origin. docs/design/data-model.md#survey--stage-4-kk0-header.
public sealed record SurveyMarketDetail(
    Guid Id,
    string? Bahan,
    Asal? Asal,
    Guid? CountryId,
    decimal? Volume,
    Guid? SatuanUnitId,
    short SortOrder);

public sealed record SaveSurveyMarketRequest(
    string? Bahan,
    Asal? Asal,
    Guid? CountryId,
    decimal? Volume,
    Guid? SatuanUnitId);
