using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.TimelineService.Handlers;

public sealed class OnConsultationStarted(ITimelineStore store) : IDomainEventHandler<ConsultationStarted>
{
    public Task HandleAsync(ConsultationStarted e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, "Doktor vakaya dahil oldu — konsültasyon başladı", e.EventId),
            ct);
}

public sealed class OnConsultationCompleted(ITimelineStore store) : IDomainEventHandler<ConsultationCompleted>
{
    public Task HandleAsync(ConsultationCompleted e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, "Konsültasyon tamamlandı", e.EventId),
            ct);
}

public sealed class OnAIAnalysisReviewed(ITimelineStore store) : IDomainEventHandler<AIAnalysisReviewed>
{
    public Task HandleAsync(AIAnalysisReviewed e, CancellationToken ct)
    {
        var summary = e.Decision == AnalysisReviewDecision.Approved
            ? "Doktor AI analizini onayladı"
            : "Doktor AI analizini düzeltti";
        return store.AppendAsync(TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, summary, e.EventId), ct);
    }
}

public sealed class OnTreatmentPlanCreated(ITimelineStore store) : IDomainEventHandler<TreatmentPlanCreated>
{
    public Task HandleAsync(TreatmentPlanCreated e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, "Doktor tedavi planı oluşturdu", e.EventId),
            ct);
}

public sealed class OnClinicalNoteAdded(ITimelineStore store) : IDomainEventHandler<ClinicalNoteAdded>
{
    public Task HandleAsync(ClinicalNoteAdded e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, "Doktor klinik not ekledi", e.EventId),
            ct);
}

/// <summary>ADR-014 MVP: "üçüncü parti inceleme talep edildi" notu timeline'a düşer.</summary>
public sealed class OnEscalationSuggested(ITimelineStore store) : IDomainEventHandler<EscalationSuggested>
{
    public Task HandleAsync(EscalationSuggested e, CancellationToken ct)
    {
        var reason = e.Reason == EscalationReason.DoctorRequested
            ? "doktor ikinci görüş talep etti"
            : "yüksek risk + doğrulanmamış görüntü bulgusu";
        return store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"Üçüncü parti inceleme önerildi ({reason}) — vaka önceliği en üst seviyeye çıkarıldı", e.EventId),
            ct);
    }
}
