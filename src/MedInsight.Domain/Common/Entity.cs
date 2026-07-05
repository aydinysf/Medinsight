namespace MedInsight.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
}
