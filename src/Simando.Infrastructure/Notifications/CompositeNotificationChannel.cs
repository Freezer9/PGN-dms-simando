using Simando.Application.Notifications;

namespace Simando.Infrastructure.Notifications;

public sealed class CompositeNotificationChannel : INotificationChannel
{
    private readonly IEnumerable<INotificationChannel> _channels;

    public CompositeNotificationChannel(IEnumerable<INotificationChannel> channels)
    {
        _channels = channels;
    }

    public async Task SendAsync(Guid recipientUserId, Guid companyId, string message, CancellationToken ct = default)
    {
        foreach (var channel in _channels)
        {
            if (channel is CompositeNotificationChannel) continue;
            try
            {
                await channel.SendAsync(recipientUserId, companyId, message, ct);
            }
            catch
            {
                // Suppress individual channel errors so side-channel failure never interrupts main workflow
            }
        }
    }
}
