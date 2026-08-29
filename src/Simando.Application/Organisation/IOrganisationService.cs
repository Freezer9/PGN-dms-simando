using Simando.Domain.Organisation;

namespace Simando.Application.Organisation;

public sealed record RegionWithAreasDto(
    Guid Id,
    string Code,
    string Name,
    bool Active,
    IReadOnlyList<AreaItemDto> Areas
);

public sealed record AreaItemDto(
    Guid Id,
    Guid RegionId,
    string Code,
    string Name,
    bool Active,
    int RecordCount = 0
);

public sealed record CreateRegionRequest(
    string Code,
    string Name
);

public sealed record UpdateRegionRequest(
    string Code,
    string Name,
    bool Active = true
);

public sealed record CreateAreaRequest(
    Guid RegionId,
    string Code,
    string Name
);

public sealed record UpdateAreaRequest(
    Guid RegionId,
    string Code,
    string Name,
    bool Active = true
);

// Bespoke, not IEntityService<T>: Region/Area aren't AuditableEntity (no
// soft-delete — a hard delete is attempted and the DB's FK-restrict rejects
// it when the row is still referenced, translated to EntityInUseException).
// Both tables are tiny (SOR I-IV, a few dozen Areas) so no paging.
public interface IOrganisationService
{
    Task<IReadOnlyList<RegionWithAreasDto>> GetOrganisationHierarchyAsync(CancellationToken ct = default);

    Task<List<Region>> GetRegionsAsync(CancellationToken ct = default);

    Task<List<Area>> GetAreasAsync(CancellationToken ct = default);

    Task AddRegionAsync(Region region, CancellationToken ct = default);

    Task UpdateRegionAsync(Guid id, Action<Region> mutate, CancellationToken ct = default);

    Task DeleteRegionAsync(Guid id, CancellationToken ct = default);

    Task AddAreaAsync(Area area, CancellationToken ct = default);

    Task UpdateAreaAsync(Guid id, Action<Area> mutate, CancellationToken ct = default);

    Task DeleteAreaAsync(Guid id, CancellationToken ct = default);
}
