using MedInsight.Application.Abstractions.Auth;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Fakes;

public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"HASH::{password}";

    public bool Verify(string password, string passwordHash) => passwordHash == $"HASH::{password}";
}

public sealed class FakeCurrentUser : ICurrentUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();

    public UserRole Role { get; set; } = UserRole.Patient;

    public bool IsAuthenticated { get; set; } = true;
}

public sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public TokenResult Generate(User user) => new($"token-{user.Id}", DateTime.UtcNow.AddHours(1));
}
