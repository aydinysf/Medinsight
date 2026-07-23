using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity;

public sealed class User : Entity
{
    private User()
    {
    }

    public string FullName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public UserStatus Status { get; private set; }

    public static User Create(string fullName, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Status = UserStatus.Active,
        };
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
