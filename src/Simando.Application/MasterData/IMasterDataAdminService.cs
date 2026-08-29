namespace Simando.Application.MasterData;

public interface IMasterDataAdminService
{
    Task<IndustryTypeResult> CreateIndustryTypeAsync(CreateIndustryTypeRequest request, CancellationToken ct = default);
    Task<bool> UpdateIndustryTypeAsync(Guid id, UpdateIndustryTypeRequest request, CancellationToken ct = default);
    Task<bool> DeleteIndustryTypeAsync(Guid id, CancellationToken ct = default);

    Task<SegmentResult> CreateSegmentAsync(CreateSegmentRequest request, CancellationToken ct = default);
    Task<bool> UpdateSegmentAsync(Guid id, UpdateSegmentRequest request, CancellationToken ct = default);
    Task<bool> DeleteSegmentAsync(Guid id, CancellationToken ct = default);

    Task<FuelTypeResult> CreateFuelTypeAsync(CreateFuelTypeRequest request, CancellationToken ct = default);
    Task<bool> UpdateFuelTypeAsync(Guid id, UpdateFuelTypeRequest request, CancellationToken ct = default);
    Task<bool> DeleteFuelTypeAsync(Guid id, CancellationToken ct = default);

    Task<UnitResult> CreateUnitAsync(CreateUnitRequest request, CancellationToken ct = default);
    Task<bool> UpdateUnitAsync(Guid id, UpdateUnitRequest request, CancellationToken ct = default);
    Task<bool> DeleteUnitAsync(Guid id, CancellationToken ct = default);

    Task<MeterSizeResult> CreateMeterSizeAsync(CreateMeterSizeRequest request, CancellationToken ct = default);
    Task<bool> UpdateMeterSizeAsync(Guid id, UpdateMeterSizeRequest request, CancellationToken ct = default);
    Task<bool> DeleteMeterSizeAsync(Guid id, CancellationToken ct = default);

    Task<MrsSpecResult> CreateMrsSpecAsync(CreateMrsSpecRequest request, CancellationToken ct = default);
    Task<bool> UpdateMrsSpecAsync(Guid id, UpdateMrsSpecRequest request, CancellationToken ct = default);
    Task<bool> DeleteMrsSpecAsync(Guid id, CancellationToken ct = default);

    Task<ReasonCategoryResult> CreateReasonCategoryAsync(CreateReasonCategoryRequest request, CancellationToken ct = default);
    Task<bool> UpdateReasonCategoryAsync(Guid id, UpdateReasonCategoryRequest request, CancellationToken ct = default);
    Task<bool> DeleteReasonCategoryAsync(Guid id, CancellationToken ct = default);

    Task<ReferenceDocumentResult> CreateReferenceDocumentAsync(CreateReferenceDocumentRequest request, CancellationToken ct = default);
    Task<bool> UpdateReferenceDocumentAsync(Guid id, UpdateReferenceDocumentRequest request, CancellationToken ct = default);
    Task<bool> DeleteReferenceDocumentAsync(Guid id, CancellationToken ct = default);
}
