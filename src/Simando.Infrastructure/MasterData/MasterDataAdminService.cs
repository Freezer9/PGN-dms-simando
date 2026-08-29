using Mapster;
using Simando.Application.Common;
using Simando.Application.MasterData;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.MasterData;

internal sealed class MasterDataAdminService(
    IEntityService<IndustryType> industryTypes,
    IEntityService<Segment> segments,
    IEntityService<FuelType> fuelTypes,
    IEntityService<UnitOfMeasure> unitsOfMeasure,
    IEntityService<MeterSize> meterSizes,
    IEntityService<MrsSpec> mrsSpecs,
    IEntityService<ReasonCategory> reasonCategories,
    IEntityService<ReferenceDocument> referenceDocuments) : IMasterDataAdminService
{
    // Industry Type
    public async Task<IndustryTypeResult> CreateIndustryTypeAsync(CreateIndustryTypeRequest request, CancellationToken ct = default)
    {
        var entity = await industryTypes.AddAsync(request.Adapt<IndustryType>(), ct);
        return entity.Adapt<IndustryTypeResult>();
    }

    public Task<bool> UpdateIndustryTypeAsync(Guid id, UpdateIndustryTypeRequest request, CancellationToken ct = default) =>
        industryTypes.UpdateAsync(id, e =>
        {
            e.Name = request.Name.Trim();
            e.ContohProduk = request.ContohProduk?.Trim();
        }, ct);

    public Task<bool> DeleteIndustryTypeAsync(Guid id, CancellationToken ct = default) =>
        industryTypes.SoftDeleteAsync(id, ct);

    // Segment
    public async Task<SegmentResult> CreateSegmentAsync(CreateSegmentRequest request, CancellationToken ct = default)
    {
        var entity = await segments.AddAsync(request.Adapt<Segment>(), ct);
        return entity.Adapt<SegmentResult>();
    }

    public Task<bool> UpdateSegmentAsync(Guid id, UpdateSegmentRequest request, CancellationToken ct = default) =>
        segments.UpdateAsync(id, e =>
        {
            e.Name = request.Name.Trim();
            e.SortOrder = request.SortOrder;
        }, ct);

    public Task<bool> DeleteSegmentAsync(Guid id, CancellationToken ct = default) =>
        segments.SoftDeleteAsync(id, ct);

    // Fuel Type
    public async Task<FuelTypeResult> CreateFuelTypeAsync(CreateFuelTypeRequest request, CancellationToken ct = default)
    {
        var entity = await fuelTypes.AddAsync(request.Adapt<FuelType>(), ct);
        return entity.Adapt<FuelTypeResult>();
    }

    public Task<bool> UpdateFuelTypeAsync(Guid id, UpdateFuelTypeRequest request, CancellationToken ct = default) =>
        fuelTypes.UpdateAsync(id, e => e.Name = request.Name.Trim(), ct);

    public Task<bool> DeleteFuelTypeAsync(Guid id, CancellationToken ct = default) =>
        fuelTypes.SoftDeleteAsync(id, ct);

    // Unit Of Measure
    public async Task<UnitResult> CreateUnitAsync(CreateUnitRequest request, CancellationToken ct = default)
    {
        var entity = await unitsOfMeasure.AddAsync(request.Adapt<UnitOfMeasure>(), ct);
        return entity.Adapt<UnitResult>();
    }

    public Task<bool> UpdateUnitAsync(Guid id, UpdateUnitRequest request, CancellationToken ct = default) =>
        unitsOfMeasure.UpdateAsync(id, e =>
        {
            e.Code = request.Code.Trim();
            e.Name = request.Name.Trim();
            e.Dimension = request.Dimension;
        }, ct);

    public Task<bool> DeleteUnitAsync(Guid id, CancellationToken ct = default) =>
        unitsOfMeasure.SoftDeleteAsync(id, ct);

    // Meter Size
    public async Task<MeterSizeResult> CreateMeterSizeAsync(CreateMeterSizeRequest request, CancellationToken ct = default)
    {
        var entity = await meterSizes.AddAsync(request.Adapt<MeterSize>(), ct);
        return entity.Adapt<MeterSizeResult>();
    }

    public Task<bool> UpdateMeterSizeAsync(Guid id, UpdateMeterSizeRequest request, CancellationToken ct = default) =>
        meterSizes.UpdateAsync(id, e =>
        {
            e.GSize = request.GSize.Trim();
            e.NominalFlow = request.NominalFlow;
            e.MaxFlow = request.MaxFlow;
            e.PressureRating = request.PressureRating;
        }, ct);

    public Task<bool> DeleteMeterSizeAsync(Guid id, CancellationToken ct = default) =>
        meterSizes.SoftDeleteAsync(id, ct);

    // MRS Spec
    public async Task<MrsSpecResult> CreateMrsSpecAsync(CreateMrsSpecRequest request, CancellationToken ct = default)
    {
        var entity = await mrsSpecs.AddAsync(request.Adapt<MrsSpec>(), ct);
        return entity.Adapt<MrsSpecResult>();
    }

    public Task<bool> UpdateMrsSpecAsync(Guid id, UpdateMrsSpecRequest request, CancellationToken ct = default) =>
        mrsSpecs.UpdateAsync(id, e => e.Name = request.Name.Trim(), ct);

    public Task<bool> DeleteMrsSpecAsync(Guid id, CancellationToken ct = default) =>
        mrsSpecs.SoftDeleteAsync(id, ct);

    // Reason Category
    public async Task<ReasonCategoryResult> CreateReasonCategoryAsync(CreateReasonCategoryRequest request, CancellationToken ct = default)
    {
        var entity = await reasonCategories.AddAsync(request.Adapt<ReasonCategory>(), ct);
        return entity.Adapt<ReasonCategoryResult>();
    }

    public Task<bool> UpdateReasonCategoryAsync(Guid id, UpdateReasonCategoryRequest request, CancellationToken ct = default) =>
        reasonCategories.UpdateAsync(id, e => e.Name = request.Name.Trim(), ct);

    public Task<bool> DeleteReasonCategoryAsync(Guid id, CancellationToken ct = default) =>
        reasonCategories.SoftDeleteAsync(id, ct);

    // Reference Document
    public async Task<ReferenceDocumentResult> CreateReferenceDocumentAsync(CreateReferenceDocumentRequest request, CancellationToken ct = default)
    {
        var entity = await referenceDocuments.AddAsync(request.Adapt<ReferenceDocument>(), ct);
        return entity.Adapt<ReferenceDocumentResult>();
    }

    public Task<bool> UpdateReferenceDocumentAsync(Guid id, UpdateReferenceDocumentRequest request, CancellationToken ct = default) =>
        referenceDocuments.UpdateAsync(id, e => e.EffectiveTo = request.EffectiveTo, ct);

    public Task<bool> DeleteReferenceDocumentAsync(Guid id, CancellationToken ct = default) =>
        referenceDocuments.UpdateAsync(id, e => e.EffectiveTo = DateOnly.FromDateTime(DateTime.UtcNow), ct);
}
