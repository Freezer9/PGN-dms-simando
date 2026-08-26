namespace Simando.Application.Common;

// Thrown by IRepository<T> when a save violates a unique constraint.
// Framework-agnostic on purpose — pages catch this instead of DbUpdateException
// so no EF type leaks past the repository, and supply their own localized message.
public sealed class DuplicateNameException : Exception;
