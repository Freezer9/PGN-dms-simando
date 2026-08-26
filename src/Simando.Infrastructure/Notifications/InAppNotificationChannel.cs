using Microsoft.EntityFrameworkCore;
using Simando.Application.Notifications;
using Simando.Domain.Notifications;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Notifications;

// Fresh-context-per-call, same shape as every other service this session.
// Intentionally not atomic with the workflow transition that triggers it —
// callers only invoke this after their own SaveChangesAsync already
// committed, so a side-channel notification failing here shouldn't (and
// structurally can't, since the interface takes no DbContext) roll back a
// real state transition.
internal sealed class InAppNotificationChannel(IDbContextFactory<SimandoDbContext> dbContextFactory) : INotificationChannel
{
    public async Task SendAsync(Guid recipientUserId, Guid companyId, string message, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        db.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            CompanyId = companyId,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(ct);
    }
}
