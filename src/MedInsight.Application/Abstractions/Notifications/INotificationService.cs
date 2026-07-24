namespace MedInsight.Application.Abstractions.Notifications;

/// <summary>Kanal seçim kuralları için bildirim türü (bkz. notification-engine.md).</summary>
public enum NotificationKind
{
    General = 0,
    CriticalRisk = 1,
    VerificationResult = 2,
    NewMessage = 3,
}

/// <summary>
/// Jenerik iletim katmanı — içerik üretmez, hazır metni kullanıcı tercihi ve
/// kanal kurallarına göre iletir. MVP: tek deneme, fallback zinciri Post-MVP.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(Guid userId, string eventType, string message, NotificationKind kind, CancellationToken cancellationToken = default);
}
