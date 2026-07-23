using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity;

public sealed class Caregiver : Entity
{
    private Caregiver()
    {
    }

    public Guid UserId { get; private set; }

    public string RelationshipType { get; private set; } = null!;

    public static Caregiver Create(Guid userId, string relationshipType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relationshipType);

        return new Caregiver
        {
            UserId = userId,
            RelationshipType = relationshipType.Trim(),
        };
    }
}
