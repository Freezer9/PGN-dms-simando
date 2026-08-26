using Microsoft.EntityFrameworkCore;
using Simando.Application.MasterData;
using Simando.Domain.MasterData;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.MasterData;

internal sealed class UnitLookupService(IDbContextFactory<SimandoDbContext> dbContextFactory) : IUnitLookupService
{
    public async Task<IReadOnlyList<UnitOption>> GetUnitsAsync(UnitSet set, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        return await db.UnitSetMembers.AsNoTracking()
            .Where(m => m.SetCode == set)
            .OrderBy(m => m.SortOrder)
            .Join(db.UnitsOfMeasure.AsNoTracking(), m => m.UnitId, u => u.Id, (m, u) => new UnitOption(u.Id, u.Code, u.Name))
            .ToListAsync(ct);
    }
}
