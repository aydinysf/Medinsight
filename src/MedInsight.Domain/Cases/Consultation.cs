using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

public enum ConsultationStatus
{
    Pending = 0,
    Active = 1,
    Completed = 2,
}

/// <summary>
/// Doktorun Case'e dahil olduğu süreç (bkz. consultation-model.md). Doktor bu
/// model üzerinden mesaj/not/tedavi planı üretir ama Case'in idari durumunu değiştiremez.
/// </summary>
public sealed class Consultation : Entity
{
    private readonly List<ConsultationMessage> _messages = [];
    private readonly List<ClinicalNote> _clinicalNotes = [];

    private Consultation()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid DoctorId { get; private set; }

    public ConsultationStatus Status { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<ConsultationMessage> Messages => _messages.AsReadOnly();

    public IReadOnlyCollection<ClinicalNote> ClinicalNotes => _clinicalNotes.AsReadOnly();

    internal static Consultation Start(Guid caseId, Guid doctorId)
    {
        return new Consultation
        {
            CaseId = caseId,
            DoctorId = doctorId,
            Status = ConsultationStatus.Active,
            StartedAtUtc = DateTime.UtcNow,
        };
    }

    internal ConsultationMessage AddMessage(Guid senderUserId, string content)
    {
        EnsureActive();
        var message = ConsultationMessage.Create(Id, senderUserId, content);
        _messages.Add(message);
        return message;
    }

    internal ClinicalNote AddClinicalNote(Guid doctorId, string content)
    {
        EnsureActive();
        if (doctorId != DoctorId)
        {
            throw new DomainException("Klinik notu yalnızca konsültasyonun doktoru yazabilir.");
        }

        var note = ClinicalNote.Create(Id, doctorId, content);
        _clinicalNotes.Add(note);
        return note;
    }

    internal void Complete()
    {
        EnsureActive();
        Status = ConsultationStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    internal void EnsureActive()
    {
        if (Status != ConsultationStatus.Active)
        {
            throw new DomainException("Konsültasyon aktif değil.");
        }
    }
}

/// <summary>İçerik at-rest column-level şifreleme gerektirir (security-architecture.md) — MVP teknik borcu.</summary>
public sealed class ConsultationMessage : Entity
{
    private ConsultationMessage()
    {
    }

    public Guid ConsultationId { get; private set; }

    public Guid SenderUserId { get; private set; }

    public string Content { get; private set; } = null!;

    internal static ConsultationMessage Create(Guid consultationId, Guid senderUserId, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new ConsultationMessage
        {
            ConsultationId = consultationId,
            SenderUserId = senderUserId,
            Content = content.Trim(),
        };
    }
}

public sealed class ClinicalNote : Entity
{
    private ClinicalNote()
    {
    }

    public Guid ConsultationId { get; private set; }

    public Guid DoctorId { get; private set; }

    public string Content { get; private set; } = null!;

    internal static ClinicalNote Create(Guid consultationId, Guid doctorId, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new ClinicalNote
        {
            ConsultationId = consultationId,
            DoctorId = doctorId,
            Content = content.Trim(),
        };
    }
}

/// <summary>Case'in doğrudan alt bileşeni; Consultation üzerinden üretilir (consultation-model.md).</summary>
public sealed class Treatment : Entity
{
    private Treatment()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid ConsultationId { get; private set; }

    public Guid CreatedByDoctorId { get; private set; }

    public string Description { get; private set; } = null!;

    public DateOnly? FollowUpDate { get; private set; }

    internal static Treatment Create(Guid caseId, Guid consultationId, Guid createdByDoctorId, string description, DateOnly? followUpDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Treatment
        {
            CaseId = caseId,
            ConsultationId = consultationId,
            CreatedByDoctorId = createdByDoctorId,
            Description = description.Trim(),
            FollowUpDate = followUpDate,
        };
    }
}
