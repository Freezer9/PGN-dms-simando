using Simando.Domain.Common;

namespace Simando.Application.Common;

// Mints repositories bound to the transaction IUnitOfWork.ExecuteInTransactionAsync
// is running — every IRepository<T> obtained from the same scope shares one
// connection/transaction, so their writes commit or roll back together.
public interface IUnitOfWorkScope
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : AuditableEntity;
}
