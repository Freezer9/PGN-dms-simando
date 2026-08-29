using Mapster;
using Microsoft.EntityFrameworkCore;
using Simando.Application.MasterData;
using Simando.Domain.MasterData;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.MasterData;

internal sealed class MasterDataAdminService(IDbContextFactory<SimandoDbContext> dbContextFactory) : IMasterDataAdminService
{
    public async Task<IndustryTypeResult> CreateIndustryTypeAsync(CreateIndustryTypeRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<IndustryType>();
        db.IndustryTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<IndustryTypeResult>();
    }

    public async Task<bool> UpdateIndustryTypeAsync(Guid id, UpdateIndustryTypeRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.IndustryTypes.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (entity is null) return false;

        entity.Name = request.Name.Trim();
        entity.ContohProduk = request.ContohProduk?.Trim();
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteIndustryTypeAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.IndustryTypes.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (entity is null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SegmentResult> CreateSegmentAsync(CreateSegmentRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<Segment>();
        db.Segments.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<SegmentResult>();
    }

    public async Task<bool> UpdateSegmentAsync(Guid id, UpdateSegmentRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.Segments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return false;

        entity.Name = request.Name.Trim();
        entity.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteSegmentAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.Segments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<FuelTypeResult> CreateFuelTypeAsync(CreateFuelTypeRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<FuelType>();
        db.FuelTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<FuelTypeResult>();
    }

    public async Task<bool> UpdateFuelTypeAsync(Guid id, UpdateFuelTypeRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.FuelTypes.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (entity is null) return false;

        entity.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteFuelTypeAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.FuelTypes.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (entity is null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UnitResult> CreateUnitAsync(CreateUnitRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<UnitOfMeasure>();
        db.UnitsOfMeasure.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<UnitResult>();
    }

    public async Task<bool> UpdateUnitAsync(Guid id, UpdateUnitRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (entity is null) return false;

        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.Dimension = request.Dimension;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteUnitAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (entity is null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<MeterSizeResult> CreateMeterSizeAsync(CreateMeterSizeRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<MeterSize>();
        db.MeterSizes.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<MeterSizeResult>();
    }

    public async Task<bool> UpdateMeterSizeAsync(Guid id, UpdateMeterSizeRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.MeterSizes.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return false;

        entity.GSize = request.GSize.Trim();
        entity.NominalFlow = request.NominalFlow;
        entity.MaxFlow = request.MaxFlow;
        entity.PressureRating = request.PressureRating;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteMeterSizeAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.MeterSizes.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<MrsSpecResult> CreateMrsSpecAsync(CreateMrsSpecRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<MrsSpec>();
        db.MrsSpecs.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<MrsSpecResult>();
    }

    public async Task<bool> UpdateMrsSpecAsync(Guid id, UpdateMrsSpecRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.MrsSpecs.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return false;

        entity.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteMrsSpecAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.MrsSpecs.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ReasonCategoryResult> CreateReasonCategoryAsync(CreateReasonCategoryRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<ReasonCategory>();
        db.ReasonCategories.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<ReasonCategoryResult>();
    }

    public async Task<bool> UpdateReasonCategoryAsync(Guid id, UpdateReasonCategoryRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.ReasonCategories.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return false;

        entity.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteReasonCategoryAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.ReasonCategories.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ReferenceDocumentResult> CreateReferenceDocumentAsync(CreateReferenceDocumentRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = request.Adapt<ReferenceDocument>();
        db.ReferenceDocuments.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Adapt<ReferenceDocumentResult>();
    }

    public async Task<bool> UpdateReferenceDocumentAsync(Guid id, UpdateReferenceDocumentRequest request, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.ReferenceDocuments.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return false;

        entity.EffectiveTo = request.EffectiveTo;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteReferenceDocumentAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var entity = await db.ReferenceDocuments.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return false;

        entity.EffectiveTo = DateOnly.FromDateTime(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
