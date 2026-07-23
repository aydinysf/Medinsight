using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity;

public sealed class User : Entity
{
    private User()
    {
    }

    public string FullName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public string PasswordHash { get; private set; } = null!;

    public UserStatus Status { get; private set; }

    public static User Create(string fullName, string email, UserRole role, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
        };
    }

    public void ChangePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    public void Suspend() => Status = UserStatus.Suspended;

    public void Activate() => Status = UserStatus.Active;
}

public enum UserStatus
{
    Active = 0,
    Suspended = 1,
    Deleted = 2,
}

/// <summary>Tek rol / kullanıcı (ADR-016; ERD: bir kullanıcı hem hasta hem doktor olamaz).</summary>
public enum UserRole
{
    Patient = 0,
    Caregiver = 1,
    Doctor = 2,
    Admin = 3,
}
