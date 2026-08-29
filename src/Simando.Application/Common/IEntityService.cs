using System.Linq.Expressions;

namespace Simando.Application.Common;

public interface IEntityService<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken ct = default);

    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        bool includeDeleted = false,
        CancellationToken ct = default);

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

    Task<bool> UpdateAsync(Guid id, Action<TEntity> mutate, CancellationToken ct = default);

    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> RestoreAsync(Guid id, CancellationToken ct = default);
}
