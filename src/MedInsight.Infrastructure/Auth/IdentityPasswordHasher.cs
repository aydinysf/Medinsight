using MedInsight.Application.Abstractions.Auth;
using MedInsight.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace MedInsight.Infrastructure.Auth;

/// <summary>ASP.NET Core Identity'nin kanıtlanmış PBKDF2 hasher'ını sarar (ADR-016).</summary>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
