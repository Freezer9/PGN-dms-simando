namespace Simando.Application.Notifications;

public sealed record NotificationListItem(
    Guid Id,
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    string Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt
);

public interface INotificationService
{
    Task<int> GetUnreadCountAsync(Guid recipientUserId, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationListItem>> GetNotificationsAsync(Guid recipientUserId, int limit = 20, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, Guid recipientUserId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid recipientUserId, CancellationToken ct = default);
}
