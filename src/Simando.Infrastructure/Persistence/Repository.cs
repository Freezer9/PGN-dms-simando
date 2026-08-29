using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Common;
using Simando.Domain.Common;

namespace Simando.Infrastructure.Persistence;

internal sealed class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly IDbContextFactory<SimandoDbContext>? _dbContextFactory;
    private readonly SimandoDbContext? _boundContext;

    public Repository(IDbContextFactory<SimandoDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

    // Used only by UnitOfWork's transaction scope — boundContext is owned
    // by the caller (the transaction), not created/disposed here.
    internal Repository(SimandoDbContext boundContext) => _boundContext = boundContext;

    public Task<TEntity?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var query = includeDeleted ? db.Set<TEntity>().IgnoreQueryFilters() : db.Set<TEntity>();
            return await query.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
        }, ct);

    public Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        bool includeDeleted = false,
        CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var query = includeDeleted ? db.Set<TEntity>().IgnoreQueryFilters() : db.Set<TEntity>();
            if (filter is not null) query = query.Where(filter);

            return await query.AsNoTracking().ToListAsync(ct);
        }, ct);

    public Task<PagedResult<TEntity>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? filter = null,
        bool includeDeleted = false,
        CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var query = includeDeleted ? db.Set<TEntity>().IgnoreQueryFilters() : db.Set<TEntity>();
            if (filter is not null) query = query.Where(filter);

            var totalCount = await query.CountAsync(ct);
            var items = await query.AsNoTracking()
                .OrderBy(orderBy)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<TEntity>(items, totalCount, page, pageSize);
        }, ct);

    public Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var query = includeDeleted ? db.Set<TEntity>().IgnoreQueryFilters() : db.Set<TEntity>();
            return await query.AnyAsync(predicate, ct);
        }, ct);

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            await SaveChangesCoreAsync(db, ct);
            return entity;
        }, ct);

    public Task<bool> UpdateAsync(Guid id, Action<TEntity> mutate, CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var entity = await db.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
            if (entity is null) return false;
            mutate(entity);
            await SaveChangesCoreAsync(db, ct);
            return true;
        }, ct);

    public Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var entity = await db.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
            if (entity is null) return false;
            if (entity is AuditableEntity auditable)
            {
                auditable.DeletedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                db.Set<TEntity>().Remove(entity);
            }
            await SaveChangesCoreAsync(db, ct);
            return true;
        }, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var entity = await db.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
            if (entity is null) return false;
            db.Set<TEntity>().Remove(entity);
            await SaveChangesCoreAsync(db, ct);
            return true;
        }, ct);

    public Task<bool> RestoreAsync(Guid id, CancellationToken ct = default) =>
        RunAsync(async db =>
        {
            var entity = await db.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
            if (entity is null) return false;
            if (entity is AuditableEntity auditable)
            {
                auditable.DeletedAt = null;
                await SaveChangesCoreAsync(db, ct);
            }
            return true;
        }, ct);

    // Opens the bound context (transaction path, not owned/disposed here)
    // or a fresh one from the factory (normal path, disposed after use) —
    // every public method runs through this so both constructors share the
    // exact same query/save logic below.
    private async Task<TResult> RunAsync<TResult>(Func<SimandoDbContext, Task<TResult>> operation, CancellationToken ct)
    {
        var (db, owned) = await OpenAsync(ct);
        try
        {
            return await operation(db);
        }
        finally
        {
            if (owned) await db.DisposeAsync();
        }
    }

    private async Task RunAsync(Func<SimandoDbContext, Task> operation, CancellationToken ct)
    {
        var (db, owned) = await OpenAsync(ct);
        try
        {
            await operation(db);
        }
        finally
        {
            if (owned) await db.DisposeAsync();
        }
    }

    private async Task<(SimandoDbContext Db, bool Owned)> OpenAsync(CancellationToken ct)
    {
        if (_boundContext is not null) return (_boundContext, false);

        return (await _dbContextFactory!.CreateDbContextAsync(ct), true);
    }

    private static Task SaveChangesCoreAsync(SimandoDbContext db, CancellationToken ct) =>
        PersistenceExceptionTranslator.SaveChangesAsync(db, ct);
}
