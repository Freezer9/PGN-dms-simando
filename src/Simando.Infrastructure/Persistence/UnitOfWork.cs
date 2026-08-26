using Microsoft.EntityFrameworkCore;
using Simando.Application.Common;
using Simando.Domain.Common;

namespace Simando.Infrastructure.Persistence;

internal sealed class UnitOfWork(IDbContextFactory<SimandoDbContext> dbContextFactory) : IUnitOfWork
{
    // Non-generic overload delegates to the generic one so the
    // transaction/rollback plumbing exists in exactly one place.
    public Task ExecuteInTransactionAsync(Func<IUnitOfWorkScope, CancellationToken, Task> operation, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(async (scope, token) =>
        {
            await operation(scope, token);
            return true;
        }, ct);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<IUnitOfWorkScope, CancellationToken, Task<TResult>> operation, CancellationToken ct = default)
    {
        // One context for the whole transaction — every IRepository<T> the
        // operation mints via the scope below shares this same connection,
        // so their writes commit or roll back together.
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async token =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(token);
            var result = await operation(new UnitOfWorkScope(db), token);
            await tx.CommitAsync(token);
            return result;
            // No explicit rollback call: if `operation` throws, control
            // never reaches CommitAsync, and `await using` disposes `tx`
            // uncommitted — IDbContextTransaction.DisposeAsync() rolls
            // back whenever Commit was never called.
        }, ct);
    }

    private sealed class UnitOfWorkScope(SimandoDbContext db) : IUnitOfWorkScope
    {
        public IRepository<TEntity> Repository<TEntity>() where TEntity : AuditableEntity => new Repository<TEntity>(db);
    }
}
