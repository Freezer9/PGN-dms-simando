using Microsoft.EntityFrameworkCore;
using Simando.Application.MasterData;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.MasterData;

internal sealed class MasterDataLookupService(IDbContextFactory<SimandoDbContext> dbContextFactory) : IMasterDataLookupService
{
    public async Task<IReadOnlyList<IndustryTypeDto>> GetIndustryTypesAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.IndustryTypes
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new IndustryTypeDto(i.Id, i.Name, i.ContohProduk))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AreaDto>> GetAreasAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var regions = await db.Regions.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name, ct);

        var areas = await db.Areas
            .AsNoTracking()
            .Where(a => a.Active)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

        return areas.Select(a => new AreaDto(
            a.Id,
            a.Name,
            a.Code,
            a.RegionId,
            regions.GetValueOrDefault(a.RegionId, "Region")
        )).ToList();
    }

    public async Task<IReadOnlyList<SalesUserDto>> GetSalesUsersAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var salesUserIds = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.Active && (a.Role == Role.SalesArea || a.Role == Role.AreaHead))
            .Select(a => a.UserId)
            .Distinct()
            .ToListAsync(ct);

        return await db.Users
            .AsNoTracking()
            .Where(u => u.Active && salesUserIds.Contains(u.Id))
            .OrderBy(u => u.FullName)
            .Select(u => new SalesUserDto(u.Id, u.FullName, u.UserName ?? string.Empty, u.Email ?? string.Empty))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FuelTypeDto>> GetFuelTypesAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.FuelTypes
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .Select(f => new FuelTypeDto(f.Id, f.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UnitOfMeasureDto>> GetUnitsAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.UnitsOfMeasure
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UnitOfMeasureDto(u.Id, u.Code, u.Name, u.Dimension))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto(c.Id, c.IsoCode, c.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SegmentDto>> GetSegmentsAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Segments
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .Select(s => new SegmentDto(s.Id, s.Name, s.SortOrder))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReferenceDocumentDto>> GetReferenceDocumentsAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.ReferenceDocuments
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new ReferenceDocumentDto(r.Id, r.Name, r.Version, r.EffectiveFrom, r.EffectiveTo))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MrsSpecDto>> GetMrsSpecsAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.MrsSpecs
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new MrsSpecDto(m.Id, m.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MeterSizeDto>> GetMeterSizesAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.MeterSizes
            .AsNoTracking()
            .OrderBy(m => m.GSize)
            .Select(m => new MeterSizeDto(m.Id, m.GSize, m.NominalFlow, m.MaxFlow, m.PressureRating))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReviewerOptionDto>> GetReviewersAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var assignments = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.Active && (a.Role == Role.Reviewer || a.Role == Role.RegionalAdmin || a.Role == Role.DivisionHead || a.Role == Role.AreaHead))
            .ToListAsync(ct);

        var userIds = assignments.Select(a => a.UserId).Distinct().ToHashSet();
        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Active && userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        return assignments
            .Where(a => users.ContainsKey(a.UserId))
            .Select(a =>
            {
                var u = users[a.UserId];
                return new ReviewerOptionDto(u.Id, u.FullName, u.UserName ?? string.Empty, u.Email ?? string.Empty, a.Role);
            })
            .OrderBy(r => r.FullName)
            .ToList();
    }

    public async Task<IReadOnlyList<ReasonCategoryDto>> GetReasonCategoriesAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.ReasonCategories
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new ReasonCategoryDto(r.Id, r.Name))
            .ToListAsync(ct);
    }
}
