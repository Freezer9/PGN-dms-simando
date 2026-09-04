using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Application.Notifications;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

public sealed record UnreadCountDto(int UnreadCount);

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    INotificationService notificationService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NotificationListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var clampedLimit = Math.Clamp(limit, 1, 100);
        var notifications = await notificationService.GetNotificationsAsync(
            currentUser.UserId,
            clampedLimit,
            ct);

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType<UnreadCountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var count = await notificationService.GetUnreadCountAsync(currentUser.UserId, ct);
        return Ok(new UnreadCountDto(count));
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        await notificationService.MarkAsReadAsync(id, currentUser.UserId, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        await notificationService.MarkAllAsReadAsync(currentUser.UserId, ct);
        return NoContent();
    }
}
