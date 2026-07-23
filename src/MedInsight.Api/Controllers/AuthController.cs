using MedInsight.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(LoginHandler login) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResultDto>> Login(Login command, CancellationToken cancellationToken)
    {
        var result = await login.HandleAsync(command, cancellationToken);
        return result is null ? Unauthorized() : result;
    }
}
