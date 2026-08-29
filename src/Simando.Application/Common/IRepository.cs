using System.Linq.Expressions;

namespace Simando.Application.Common;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken ct = default);

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

    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        CancellationToken ct = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);

    // Fetch-mutate-save as one call: each repository call opens its own short-lived
    // DbContext (see Repository<T>), so the fetch and the save happen inside the
    // same call to act on the same context with translated persistence exceptions.
    Task<bool> UpdateAsync(Guid id, Action<TEntity> mutate, CancellationToken ct = default);

    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> RestoreAsync(Guid id, CancellationToken ct = default);
}
