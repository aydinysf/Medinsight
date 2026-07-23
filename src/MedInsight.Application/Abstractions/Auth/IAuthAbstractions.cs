using MedInsight.Domain.Identity;

namespace MedInsight.Application.Abstractions.Auth;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}

public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    TokenResult Generate(User user);
}

/// <summary>Kaynak bazlı yetki kontrolleri için istekteki kimlik (ADR-016, katman 2).</summary>
public interface ICurrentUser
{
    Guid UserId { get; }

    UserRole Role { get; }

    bool IsAuthenticated { get; }
}
