using MedInsight.Domain.Common;
using MedInsight.Domain.Enums;

namespace MedInsight.Domain.Entities;

public sealed class TimelineEvent : Entity
{
    private TimelineEvent()
    {
    }

    public Guid MedicalCaseId { get; private set; }

    public TimelineEventType Type { get; private set; }

    public DateTime EventDateUtc { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public static TimelineEvent Create(
        Guid medicalCaseId,
        TimelineEventType type,
        DateTime eventDateUtc,
        string title,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new TimelineEvent
        {
            MedicalCaseId = medicalCaseId,
            Type = type,
            EventDateUtc = DateTime.SpecifyKind(eventDateUtc, DateTimeKind.Utc),
            Title = title.Trim(),
            Description = description,
        };
    }
}
