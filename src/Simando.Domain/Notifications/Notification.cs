namespace Simando.Domain.Notifications;

// Bare entity, same reasoning as StatusEvent/WorkflowStep — append-and-mutate
// one field (ReadAt), no soft delete. Written by INotificationChannel
// implementations; read side (bell panel) is a separate, not-yet-built card.
public sealed class Notification
{
    public required Guid Id { get; init; }
    public required Guid RecipientUserId { get; init; }
    public required Guid CompanyId { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; set; }
}
