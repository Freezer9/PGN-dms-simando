using System.Linq.Expressions;
using Simando.Domain.Common;

namespace Simando.Application.Common;

public interface IRepository<TEntity> where TEntity : AuditableEntity
{
    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        bool includeDeleted = false,
        CancellationToken ct = default);

    // orderBy is required, not optional: Postgres gives no row-order
    // guarantee for Skip/Take without an ORDER BY, so a generic paged
    // query with no per-entity-known default sort would silently return
    // unstable pages.
    Task<PagedResult<TEntity>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? filter = null,
        bool includeDeleted = false,
        CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);

    // Fetch-mutate-save as one call, not GetForEditAsync + a separate
    // SaveChangesAsync: each repository call opens its own short-lived
    // DbContext (see Repository<T>), so the fetch and the save must happen
    // inside the same call to act on the same context.
    Task UpdateAsync(Guid id, Action<TEntity> mutate, CancellationToken ct = default);

    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);

    Task RestoreAsync(Guid id, CancellationToken ct = default);
}
