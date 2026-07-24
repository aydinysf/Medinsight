using MedInsight.Application.Abstractions.Auth;
using MedInsight.Infrastructure.Notifications;
using MedInsight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/users/me/notifications")]
[Authorize]
public sealed class NotificationsController(MedInsightDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NotificationLog>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationLog>>> GetMine([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var logs = await db.NotificationLogs.AsNoTracking()
            .Where(n => n.UserId == currentUser.UserId)
            .OrderByDescending(n => n.SentAtUtc)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }
}
