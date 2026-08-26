using System.Linq.Expressions;
using Simando.Domain.Common;

namespace Simando.Application.Common;

// What Razor pages inject for simple lookup CRUD. Deliberately not the
// only path into the data: a future bespoke multi-entity workflow service
// (e.g. something in the Company/workflow area) is hand-written against
// IRepository<T> + IUnitOfWork directly rather than being forced through
// this generic type — this only covers the "simple lookup CRUD" shape.
public interface IEntityService<TEntity> where TEntity : AuditableEntity
{
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

    Task AddAsync(TEntity entity, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Action<TEntity> mutate, CancellationToken ct = default);

    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);

    Task RestoreAsync(Guid id, CancellationToken ct = default);
}
