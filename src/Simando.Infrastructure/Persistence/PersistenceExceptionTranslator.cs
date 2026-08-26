using Microsoft.EntityFrameworkCore;
using Npgsql;
using Simando.Application.Common;

namespace Simando.Infrastructure.Persistence;

// Shared by Repository<T> and OrganisationService (which can't go through
// Repository<T> — Region/Area aren't AuditableEntity) so both translate
// DbUpdateException into the same framework-agnostic domain exceptions.
internal static class PersistenceExceptionTranslator
{
    public static async Task SaveChangesAsync(SimandoDbContext db, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // A failed SaveChangesAsync leaves entities tracked in their
            // pre-save state (EF Core does not auto-detach on failure) — an
            // unrelated later save on this same context would otherwise
            // silently retry persisting them once whatever conflict caused
            // this failure no longer applies.
            db.ChangeTracker.Clear();
            throw new DuplicateNameException();
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            db.ChangeTracker.Clear();
            throw new EntityInUseException();
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    // NO ACTION violations raise 23503 (foreign_key_violation); EF Core's
    // DeleteBehavior.Restrict maps to an actual SQL ON DELETE RESTRICT
    // clause, which Postgres reports as 23001 (restrict_violation) instead
    // — both mean the same thing to a caller: the row is still referenced.
    private static bool IsForeignKeyViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.RestrictViolation,
        };
}
