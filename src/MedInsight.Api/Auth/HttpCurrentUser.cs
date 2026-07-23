using System.Security.Claims;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Domain.Identity;

namespace MedInsight.Api.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("Kimliği doğrulanmış istek bekleniyordu.");

    public UserRole Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue(ClaimTypes.Role), out var role)
            ? role
            : throw new InvalidOperationException("Kimliği doğrulanmış istek bekleniyordu.");
}
