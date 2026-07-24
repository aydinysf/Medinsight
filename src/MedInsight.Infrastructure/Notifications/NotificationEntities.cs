namespace MedInsight.Infrastructure.Notifications;

/// <summary>Kullanıcı bazlı kanal tercihi (notification-engine.md).</summary>
public sealed class NotificationPreference
{
    public Guid UserId { get; init; }

    public bool PushEnabled { get; set; } = true;

    public bool SmsEnabled { get; set; }

    public bool EmailEnabled { get; set; } = true;
}

public sealed class NotificationLog
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid UserId { get; init; }

    public string EventType { get; init; } = null!;

    public string Channel { get; init; } = null!;

    public string Message { get; init; } = null!;

    public DateTime SentAtUtc { get; init; } = DateTime.UtcNow;

    public string DeliveryStatus { get; init; } = null!;
}
