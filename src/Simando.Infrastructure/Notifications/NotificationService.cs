using Microsoft.EntityFrameworkCore;
using Simando.Application.Notifications;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Notifications;

internal sealed class NotificationService(IDbContextFactory<SimandoDbContext> dbContextFactory)
    : INotificationService
{
    public async Task<int> GetUnreadCountAsync(Guid recipientUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.RecipientUserId == recipientUserId && n.ReadAt == null, ct);
    }

    public async Task<IReadOnlyList<NotificationListItem>> GetNotificationsAsync(Guid recipientUserId, int limit = 20, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var notifications = await db.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == recipientUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        if (notifications.Count == 0) return [];

        var companyIds = notifications.Select(n => n.CompanyId).Distinct().ToList();
        var companyMap = await db.Companies
            .AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => new { c.Nomor, c.NamaPerusahaan }, ct);

        return notifications.Select(n =>
        {
            var company = companyMap.GetValueOrDefault(n.CompanyId);
            return new NotificationListItem(
                n.Id,
                n.CompanyId,
                company?.Nomor ?? "-",
                company?.NamaPerusahaan ?? "Perusahaan",
                n.Message,
                n.CreatedAt,
                n.ReadAt
            );
        }).ToList();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid recipientUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == recipientUserId, ct);

        if (notification is not null && notification.ReadAt is null)
        {
            notification.ReadAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(Guid recipientUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var unread = await db.Notifications
            .Where(n => n.RecipientUserId == recipientUserId && n.ReadAt == null)
            .ToListAsync(ct);

        if (unread.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var n in unread)
            {
                n.ReadAt = now;
            }
            await db.SaveChangesAsync(ct);
        }
    }
}
