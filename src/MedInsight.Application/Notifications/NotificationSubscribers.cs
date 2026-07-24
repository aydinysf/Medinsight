using MedInsight.Application.Abstractions.Notifications;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;
using MedInsight.Domain.Identity.Events;

namespace MedInsight.Application.Notifications;

/// <summary>
/// Notification Engine aboneleri. Not (bilinen MVP sapması): bildirim metni event
/// payload'ında hazır gelmediği için abone kenarında üretilir — iletim katmanı
/// (INotificationService) jenerik kalır (notification-engine.md ilkesi korunur).
/// </summary>
public static class CaseRecipients
{
    /// <summary>Vaka olayları hasta tarafına gider: Patient + Caregiver üyeler.</summary>
    public static IEnumerable<Guid> PatientSide(Case medicalCase) =>
        medicalCase.Members
            .Where(m => m.Role is CaseRole.Patient or CaseRole.Caregiver)
            .Select(m => m.UserId);
}

public sealed class NotifyOnDocumentClassificationFailed(ICaseRepository cases, INotificationService notifications)
    : IDomainEventHandler<DocumentClassificationFailed>
{
    public async Task HandleAsync(DocumentClassificationFailed e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null)
        {
            return;
        }

        foreach (var userId in CaseRecipients.PatientSide(medicalCase))
        {
            await notifications.NotifyAsync(userId, e.EventType,
                "Yüklediğin dosyalardan biri tanınamadı. Farklı bir formatta (PDF, DICOM veya fotoğraf) tekrar yükler misin?",
                NotificationKind.General, cancellationToken);
        }
    }
}

public sealed class NotifyOnAIAnalysisCompleted(ICaseRepository cases, INotificationService notifications)
    : IDomainEventHandler<AIAnalysisCompleted>
{
    public async Task HandleAsync(AIAnalysisCompleted e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        var analysis = medicalCase?.AiAnalyses.FirstOrDefault(a => a.Id == e.AnalysisId);
        if (medicalCase is null || analysis is null)
        {
            return;
        }

        foreach (var userId in CaseRecipients.PatientSide(medicalCase))
        {
            // İçerik Hızır'ın persona katmanından geliyor (PatientMessage) — dil kuralları orada uygulanır.
            await notifications.NotifyAsync(userId, e.EventType, analysis.PatientMessage, NotificationKind.General, cancellationToken);
        }
    }
}

/// <summary>ADR-004'ün hasta bildirimi dalı — doktor önceliklendirmesinden bağımsız işler.</summary>
public sealed class NotifyOnDoctorReviewPriorityRaised(ICaseRepository cases, INotificationService notifications)
    : IDomainEventHandler<DoctorReviewPriorityRaised>
{
    public async Task HandleAsync(DoctorReviewPriorityRaised e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null)
        {
            return;
        }

        foreach (var userId in CaseRecipients.PatientSide(medicalCase))
        {
            await notifications.NotifyAsync(userId, e.EventType,
                "Vakan öncelikli olarak doktor inceleme sırasına alındı — değerlendirme netleşince haber vereceğiz.",
                NotificationKind.CriticalRisk, cancellationToken);
        }
    }
}

public sealed class NotifyOnConsultationMessageSent(ICaseRepository cases, INotificationService notifications)
    : IDomainEventHandler<ConsultationMessageSent>
{
    public async Task HandleAsync(ConsultationMessageSent e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null)
        {
            return;
        }

        // İçerik taşınmaz — gizlilik (domain-events-catalog.md).
        foreach (var member in medicalCase.Members.Where(m => m.UserId != e.SenderUserId))
        {
            await notifications.NotifyAsync(member.UserId, e.EventType, "Konsültasyonda yeni bir mesajın var.", NotificationKind.NewMessage, cancellationToken);
        }
    }
}

public sealed class NotifyOnDoctorVerified(IDoctorRepository doctors, INotificationService notifications)
    : IDomainEventHandler<DoctorVerified>
{
    public async Task HandleAsync(DoctorVerified e, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(e.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return;
        }

        await notifications.NotifyAsync(doctor.UserId, e.EventType,
            "Doktor doğrulamanız onaylandı — artık vakalara katılabilirsiniz.",
            NotificationKind.VerificationResult, cancellationToken);
    }
}

public sealed class NotifyOnDoctorVerificationRejected(IDoctorRepository doctors, INotificationService notifications)
    : IDomainEventHandler<DoctorVerificationRejected>
{
    public async Task HandleAsync(DoctorVerificationRejected e, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(e.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return;
        }

        await notifications.NotifyAsync(doctor.UserId, e.EventType,
            $"Doktor doğrulama başvurunuz reddedildi: {e.RejectionReason}",
            NotificationKind.VerificationResult, cancellationToken);
    }
}
