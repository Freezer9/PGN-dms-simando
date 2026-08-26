using Microsoft.EntityFrameworkCore;
using Simando.Application.Geography;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Geography;

// Fresh-context-per-call, same shape as every other service. Plain lookups —
// no scope filtering, geography is shared reference data.
internal sealed class GeographyService(IDbContextFactory<SimandoDbContext> dbContextFactory) : IGeographyService
{
    public async Task<IReadOnlyList<GeographyOption>> GetProvincesAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Provinces.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new GeographyOption(p.Id, p.Name, p.BpsCode))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GeographyOption>> GetRegenciesAsync(Guid provinceId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Regencies.AsNoTracking()
            .Where(r => r.ProvinceId == provinceId)
            .OrderBy(r => r.Name)
            .Select(r => new GeographyOption(r.Id, r.Name, r.BpsCode))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GeographyOption>> GetDistrictsAsync(Guid regencyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Districts.AsNoTracking()
            .Where(d => d.RegencyId == regencyId)
            .OrderBy(d => d.Name)
            .Select(d => new GeographyOption(d.Id, d.Name, d.BpsCode))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GeographyOption>> GetVillagesAsync(Guid districtId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Villages.AsNoTracking()
            .Where(v => v.DistrictId == districtId)
            .OrderBy(v => v.Name)
            .Select(v => new GeographyOption(v.Id, v.Name, v.BpsCode))
            .ToListAsync(ct);
    }
}
