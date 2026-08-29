using System.Linq.Expressions;

namespace Simando.Application.Common;

// Forwards 1:1 to IRepository<TEntity> — no transaction needed here since
// each method is already a single, self-contained repository call.
public class EntityService<TEntity>(IRepository<TEntity> repository) : IEntityService<TEntity>
    where TEntity : class
{
    public Task<TEntity?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken ct = default) =>
        repository.GetByIdAsync(id, includeDeleted, ct);

    public Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        bool includeDeleted = false,
        CancellationToken ct = default) =>
        repository.GetAllAsync(filter, includeDeleted, ct);

    public Task<PagedResult<TEntity>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? filter = null,
        bool includeDeleted = false,
        CancellationToken ct = default) =>
        repository.GetPagedAsync(page, pageSize, orderBy, filter, includeDeleted, ct);

    public Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        CancellationToken ct = default) =>
        repository.ExistsAsync(predicate, includeDeleted, ct);

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default) =>
        repository.AddAsync(entity, ct);

    public Task<bool> UpdateAsync(Guid id, Action<TEntity> mutate, CancellationToken ct = default) =>
        repository.UpdateAsync(id, mutate, ct);

    public Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default) =>
        repository.SoftDeleteAsync(id, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        repository.DeleteAsync(id, ct);

    public Task<bool> RestoreAsync(Guid id, CancellationToken ct = default) =>
        repository.RestoreAsync(id, ct);
}
