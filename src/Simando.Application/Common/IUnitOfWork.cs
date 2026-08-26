namespace Simando.Application.Common;

public interface IUnitOfWork
{
    // Non-generic overload is for callers with nothing to return from the operation.
    Task ExecuteInTransactionAsync(Func<IUnitOfWorkScope, CancellationToken, Task> operation, CancellationToken ct = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<IUnitOfWorkScope, CancellationToken, Task<TResult>> operation, CancellationToken ct = default);
}
