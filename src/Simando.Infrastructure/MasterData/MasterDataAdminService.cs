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
        var entity = new IndustryType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ContohProduk = request.ContohProduk?.Trim()
        };
        db.IndustryTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return new IndustryTypeResult(entity.Id, entity.Name, entity.ContohProduk);
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
        var entity = new Segment
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            SortOrder = request.SortOrder
        };
        db.Segments.Add(entity);
        await db.SaveChangesAsync(ct);
        return new SegmentResult(entity.Id, entity.Name, entity.SortOrder);
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
        var entity = new FuelType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };
        db.FuelTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return new FuelTypeResult(entity.Id, entity.Name);
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
        var entity = new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Dimension = request.Dimension
        };
        db.UnitsOfMeasure.Add(entity);
        await db.SaveChangesAsync(ct);
        return new UnitResult(entity.Id, entity.Code, entity.Name, entity.Dimension);
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
        var entity = new MeterSize
        {
            Id = Guid.NewGuid(),
            GSize = request.GSize.Trim(),
            NominalFlow = request.NominalFlow,
            MaxFlow = request.MaxFlow,
            PressureRating = request.PressureRating
        };
        db.MeterSizes.Add(entity);
        await db.SaveChangesAsync(ct);
        return new MeterSizeResult(entity.Id, entity.GSize, entity.NominalFlow, entity.MaxFlow, entity.PressureRating);
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
        var entity = new MrsSpec
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };
        db.MrsSpecs.Add(entity);
        await db.SaveChangesAsync(ct);
        return new MrsSpecResult(entity.Id, entity.Name);
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
        var entity = new ReasonCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };
        db.ReasonCategories.Add(entity);
        await db.SaveChangesAsync(ct);
        return new ReasonCategoryResult(entity.Id, entity.Name);
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
        var entity = new ReferenceDocument
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Version = request.Version,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            BlobKey = request.BlobKey
        };
        db.ReferenceDocuments.Add(entity);
        await db.SaveChangesAsync(ct);
        return new ReferenceDocumentResult(entity.Id, entity.Name, entity.Version, entity.EffectiveFrom, entity.EffectiveTo, entity.BlobKey);
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
