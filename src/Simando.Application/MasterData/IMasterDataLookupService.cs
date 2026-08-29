using Simando.Domain.MasterData;
using Simando.Domain.Security;

namespace Simando.Application.MasterData;

public sealed record IndustryTypeDto(Guid Id, string Name, string? ContohProduk);

public sealed record AreaDto(Guid Id, string Name, string Code, Guid RegionId, string RegionName);

public sealed record SalesUserDto(Guid Id, string FullName, string Username, string Email);

public sealed record FuelTypeDto(Guid Id, string Name);

public sealed record UnitOfMeasureDto(Guid Id, string Code, string Name, UnitDimension Dimension);

public sealed record CountryDto(Guid Id, string IsoCode, string Name);

public sealed record SegmentDto(Guid Id, string Name, int SortOrder);

public sealed record ReferenceDocumentDto(Guid Id, string Name, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo);

public sealed record MrsSpecDto(Guid Id, string Name);

public sealed record MeterSizeDto(Guid Id, string GSize, decimal NominalFlow, decimal MaxFlow, decimal PressureRating);

public sealed record ReviewerOptionDto(Guid Id, string FullName, string Username, string Email, Role Role);

public sealed record ReasonCategoryDto(Guid Id, string Name);

public interface IMasterDataLookupService
{
    Task<IReadOnlyList<IndustryTypeDto>> GetIndustryTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AreaDto>> GetAreasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SalesUserDto>> GetSalesUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FuelTypeDto>> GetFuelTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UnitOfMeasureDto>> GetUnitsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SegmentDto>> GetSegmentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReferenceDocumentDto>> GetReferenceDocumentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MrsSpecDto>> GetMrsSpecsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MeterSizeDto>> GetMeterSizesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReviewerOptionDto>> GetReviewersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReasonCategoryDto>> GetReasonCategoriesAsync(CancellationToken ct = default);
}
