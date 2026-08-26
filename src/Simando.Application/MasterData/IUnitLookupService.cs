using Simando.Domain.MasterData;

namespace Simando.Application.MasterData;

// UnitOfMeasure doesn't carry its own UnitSet — that only lives on the
// UnitSetMember join table, which IEntityService<T> (single-entity, no
// joins) can't express. Small bespoke lookup instead.
public interface IUnitLookupService
{
    Task<IReadOnlyList<UnitOption>> GetUnitsAsync(UnitSet set, CancellationToken ct = default);
}

public sealed record UnitOption(Guid Id, string Code, string Name);
