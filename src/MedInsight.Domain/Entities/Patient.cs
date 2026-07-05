using MedInsight.Domain.Common;
using MedInsight.Domain.Enums;

namespace MedInsight.Domain.Entities;

public sealed class Patient : Entity
{
    private readonly List<MedicalCase> _cases = [];

    private Patient()
    {
    }

    public string FullName { get; private set; } = null!;

    public DateOnly? BirthDate { get; private set; }

    public Sex Sex { get; private set; }

    public IReadOnlyCollection<MedicalCase> Cases => _cases.AsReadOnly();

    public static Patient Create(string fullName, DateOnly? birthDate = null, Sex sex = Sex.Unknown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        return new Patient
        {
            FullName = fullName.Trim(),
            BirthDate = birthDate,
            Sex = sex,
        };
    }
}
