using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Auth;

public sealed record Login(
    [Required] [EmailAddress] string Email,
    [Required] string Password);

public sealed record LoginResultDto(string AccessToken, DateTime ExpiresAtUtc, Guid UserId, UserRole Role);

public sealed class LoginHandler(IUserRepository users, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
{
    /// <summary>Kimlik doğrulanamazsa null döner (API 401'e eşler; kullanıcı/parola ayrımı sızdırılmaz).</summary>
    public async Task<LoginResultDto?> HandleAsync(Login command, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null || user.Status != UserStatus.Active || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return null;
        }

        var token = tokenGenerator.Generate(user);
        return new LoginResultDto(token.AccessToken, token.ExpiresAtUtc, user.Id, user.Role);
    }
}
