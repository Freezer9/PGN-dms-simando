namespace Simando.Application.Notifications;

// The seam docs/build/testing.md's T4 protects: enabling email later means
// registering a different INotificationChannel in DI, nothing else changes.
// Deliberately no DB types in the signature so an email implementation is a
// drop-in.
public interface INotificationChannel
{
    Task SendAsync(Guid recipientUserId, Guid companyId, string message, CancellationToken ct = default);
}
