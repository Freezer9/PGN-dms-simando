namespace Simando.Application.Common;

// Thrown when a hard delete violates a foreign-key restrict constraint —
// the row is still referenced elsewhere and must be deactivated instead.
// Framework-agnostic on purpose, same reasoning as DuplicateNameException.
public sealed class EntityInUseException : Exception;
