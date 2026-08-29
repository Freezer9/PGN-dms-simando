using Simando.Domain.MasterData;

namespace Simando.Application.MasterData;

public sealed record CreateIndustryTypeRequest(string Name, string? ContohProduk);
public sealed record UpdateIndustryTypeRequest(string Name, string? ContohProduk);
public sealed record IndustryTypeResult(Guid Id, string Name, string? ContohProduk);

public sealed record CreateSegmentRequest(string Name, int SortOrder);
public sealed record UpdateSegmentRequest(string Name, int SortOrder);
public sealed record SegmentResult(Guid Id, string Name, int SortOrder);

public sealed record CreateFuelTypeRequest(string Name);
public sealed record UpdateFuelTypeRequest(string Name);
public sealed record FuelTypeResult(Guid Id, string Name);

public sealed record CreateUnitRequest(string Code, string Name, UnitDimension Dimension);
public sealed record UpdateUnitRequest(string Code, string Name, UnitDimension Dimension);
public sealed record UnitResult(Guid Id, string Code, string Name, UnitDimension Dimension);

public sealed record CreateMeterSizeRequest(string GSize, decimal NominalFlow, decimal MaxFlow, decimal PressureRating);
public sealed record UpdateMeterSizeRequest(string GSize, decimal NominalFlow, decimal MaxFlow, decimal PressureRating);
public sealed record MeterSizeResult(Guid Id, string GSize, decimal NominalFlow, decimal MaxFlow, decimal PressureRating);

public sealed record CreateMrsSpecRequest(string Name);
public sealed record UpdateMrsSpecRequest(string Name);
public sealed record MrsSpecResult(Guid Id, string Name);

public sealed record CreateReasonCategoryRequest(string Name);
public sealed record UpdateReasonCategoryRequest(string Name);
public sealed record ReasonCategoryResult(Guid Id, string Name);

public sealed record CreateReferenceDocumentRequest(string Name, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? BlobKey = null);
public sealed record UpdateReferenceDocumentRequest(string Name, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? BlobKey = null);
public sealed record ReferenceDocumentResult(Guid Id, string Name, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? BlobKey);
