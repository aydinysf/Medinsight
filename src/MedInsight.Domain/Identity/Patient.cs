using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity;

public sealed class Patient : Entity
{
    private Patient()
    {
    }

    public Guid UserId { get; private set; }

    public DateOnly? DateOfBirth { get; private set; }

    public Sex Sex { get; private set; }

    public static Patient Create(Guid userId, DateOnly? dateOfBirth = null, Sex sex = Sex.Unknown)
    {
        return new Patient
        {
            UserId = userId,
            DateOfBirth = dateOfBirth,
            Sex = sex,
        };
    }
}

public enum Sex
{
    Unknown = 0,
    Female = 1,
    Male = 2,
    Other = 3,
}
