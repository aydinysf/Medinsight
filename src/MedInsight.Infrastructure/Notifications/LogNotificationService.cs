using MedInsight.Application.Abstractions.Notifications;
using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Notifications;

/// <summary>
/// MVP iletim katmanı: gerçek push/SMS/e-posta sağlayıcısı bağlanana kadar
/// bildirimler NotificationLog'a "Simulated" olarak yazılır. Kanal seçim
/// kuralları (notification-engine.md) burada uygulanır; sağlayıcı bağlandığında
/// yalnızca gönderim adımı değişir. Fallback zinciri Post-MVP (bilinen sınırlama).
/// </summary>
public sealed class LogNotificationService(MedInsightDbContext db) : INotificationService
{
    public async Task NotifyAsync(Guid userId, string eventType, string message, NotificationKind kind, CancellationToken cancellationToken = default)
    {
        var preference = await db.Set<NotificationPreference>().AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? new NotificationPreference { UserId = userId };

        foreach (var channel in ResolveChannels(kind, preference))
        {
            db.Set<NotificationLog>().Add(new NotificationLog
            {
                UserId = userId,
                EventType = eventType,
                Channel = channel,
                Message = message,
                DeliveryStatus = "Simulated", // TODO(notification-provider): gerçek sağlayıcı entegrasyonu
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Kanal seçim kuralları — notification-engine.md tablosu.</summary>
    private static IEnumerable<string> ResolveChannels(NotificationKind kind, NotificationPreference preference)
    {
        switch (kind)
        {
            case NotificationKind.CriticalRisk:
                if (preference.PushEnabled)
                {
                    yield return "Push";
                }

                if (preference.SmsEnabled)
                {
                    yield return "Sms";
                }

                break;

            case NotificationKind.VerificationResult:
                if (preference.PushEnabled)
                {
                    yield return "Push";
                }

                if (preference.EmailEnabled)
                {
                    yield return "Email";
                }

                break;

            default:
                if (preference.PushEnabled)
                {
                    yield return "Push";
                }

                break;
        }
    }
}
