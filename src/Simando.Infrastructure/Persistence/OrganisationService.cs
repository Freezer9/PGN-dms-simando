using Microsoft.EntityFrameworkCore;
using Simando.Application.Organisation;
using Simando.Domain.Organisation;

namespace Simando.Infrastructure.Persistence;

// Bespoke, not Repository<T> — Region/Area aren't AuditableEntity, so they
// can't satisfy IRepository<T>'s constraint. Same fresh-context-per-call
// shape as Repository<T> though: each method opens and disposes its own
// SimandoDbContext via the factory.
internal sealed class OrganisationService(IDbContextFactory<SimandoDbContext> dbContextFactory) : IOrganisationService
{
    public async Task<IReadOnlyList<RegionWithAreasDto>> GetOrganisationHierarchyAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var regions = await db.Regions.AsNoTracking().ToListAsync(ct);
        var areas = await db.Areas.AsNoTracking().ToListAsync(ct);

        var areaCounts = await db.Companies.IgnoreQueryFilters().AsNoTracking()
            .GroupBy(c => c.AreaId)
            .Select(g => new { AreaId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AreaId, x => x.Count, ct);

        var areasByRegion = areas.GroupBy(a => a.RegionId).ToDictionary(
            g => g.Key,
            g => g.Select(a => new AreaItemDto(
                a.Id,
                a.RegionId,
                a.Code,
                a.Name,
                a.Active,
                areaCounts.GetValueOrDefault(a.Id, 0)
            )).OrderBy(a => a.Name).ToList()
        );

        return regions.Select(r => new RegionWithAreasDto(
            r.Id,
            r.Code,
            r.Name,
            r.Active,
            areasByRegion.GetValueOrDefault(r.Id, [])
        )).OrderBy(r => r.Name).ToList();
    }

    public async Task<List<Region>> GetRegionsAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Regions.AsNoTracking().ToListAsync(ct);
    }

    public async Task<List<Area>> GetAreasAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Areas.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddRegionAsync(Region region, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        db.Regions.Add(region);
        await PersistenceExceptionTranslator.SaveChangesAsync(db, ct);
    }

    public async Task UpdateRegionAsync(Guid id, Action<Region> mutate, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var region = await db.Regions.FirstAsync(r => r.Id == id, ct);
        mutate(region);
        await PersistenceExceptionTranslator.SaveChangesAsync(db, ct);
    }

    public async Task DeleteRegionAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var region = await db.Regions.FirstAsync(r => r.Id == id, ct);
        db.Regions.Remove(region);
        await PersistenceExceptionTranslator.SaveChangesAsync(db, ct);
    }

    public async Task AddAreaAsync(Area area, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        db.Areas.Add(area);
        await PersistenceExceptionTranslator.SaveChangesAsync(db, ct);
    }

    public async Task UpdateAreaAsync(Guid id, Action<Area> mutate, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var area = await db.Areas.FirstAsync(a => a.Id == id, ct);
        mutate(area);
        await PersistenceExceptionTranslator.SaveChangesAsync(db, ct);
    }

    public async Task DeleteAreaAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var area = await db.Areas.FirstAsync(a => a.Id == id, ct);
        db.Areas.Remove(area);
        await PersistenceExceptionTranslator.SaveChangesAsync(db, ct);
    }
}
