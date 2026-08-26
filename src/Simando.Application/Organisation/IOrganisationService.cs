using Simando.Domain.Organisation;

namespace Simando.Application.Organisation;

// Bespoke, not IEntityService<T>: Region/Area aren't AuditableEntity (no
// soft-delete — a hard delete is attempted and the DB's FK-restrict rejects
// it when the row is still referenced, translated to EntityInUseException).
// Both tables are tiny (SOR I-IV, a few dozen Areas) so no paging.
public interface IOrganisationService
{
    Task<List<Region>> GetRegionsAsync(CancellationToken ct = default);

    Task<List<Area>> GetAreasAsync(CancellationToken ct = default);

    Task AddRegionAsync(Region region, CancellationToken ct = default);

    Task UpdateRegionAsync(Guid id, Action<Region> mutate, CancellationToken ct = default);

    Task DeleteRegionAsync(Guid id, CancellationToken ct = default);

    Task AddAreaAsync(Area area, CancellationToken ct = default);

    Task UpdateAreaAsync(Guid id, Action<Area> mutate, CancellationToken ct = default);

    Task DeleteAreaAsync(Guid id, CancellationToken ct = default);
}
