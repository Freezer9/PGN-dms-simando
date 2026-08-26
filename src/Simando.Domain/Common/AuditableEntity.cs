namespace Simando.Domain.Common;

public abstract class AuditableEntity
{
    public required Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
}
