using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

/// <summary>
/// Tek aggregate root (ADR-001). Durum makinesi: docs/domain/case-lifecycle-state-machine.md.
/// Hiçbir durum değişimi sessiz gerçekleşmez — her geçiş CaseStatusChanged üretir.
/// </summary>
public sealed class Case : AggregateRoot
{
    private static readonly IReadOnlyDictionary<CaseStatus, CaseStatus[]> AllowedTransitions =
        new Dictionary<CaseStatus, CaseStatus[]>
        {
            [CaseStatus.Draft] = [CaseStatus.CollectingData],
            [CaseStatus.CollectingData] = [CaseStatus.AIAnalysis],
            [CaseStatus.AIAnalysis] = [CaseStatus.DoctorReview],
            [CaseStatus.DoctorReview] = [CaseStatus.Treatment],
            [CaseStatus.Treatment] = [CaseStatus.FollowUp],
            [CaseStatus.FollowUp] = [CaseStatus.CollectingData, CaseStatus.Closed],
            [CaseStatus.Closed] = [CaseStatus.FollowUp],
        };

    private readonly List<CaseMember> _members = [];
    private readonly List<MedicalDocument> _documents = [];
    private readonly List<DicomStudy> _dicomStudies = [];
    private readonly List<Measurement> _measurements = [];

    private Case()
    {
    }

    public Guid PatientId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public BodySystem BodySystem { get; private set; }

    public CaseStatus Status { get; private set; }

    public RiskLevel RiskLevel { get; private set; }

    public IReadOnlyCollection<CaseMember> Members => _members.AsReadOnly();

    public IReadOnlyCollection<MedicalDocument> Documents => _documents.AsReadOnly();

    public IReadOnlyCollection<DicomStudy> DicomStudies => _dicomStudies.AsReadOnly();

    public IReadOnlyCollection<Measurement> Measurements => _measurements.AsReadOnly();

    public static Case Create(Guid patientId, Guid patientUserId, string title, BodySystem bodySystem = BodySystem.Unknown, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var medicalCase = new Case
        {
            PatientId = patientId,
            Title = title.Trim(),
            Description = description,
            BodySystem = bodySystem,
            Status = CaseStatus.Draft,
            RiskLevel = RiskLevel.Unknown,
        };

        medicalCase._members.Add(CaseMember.Create(medicalCase.Id, patientUserId, CaseRole.Patient, PermissionLevel.Manage));
        medicalCase.Raise(new CaseCreated { CaseId = medicalCase.Id, PatientId = patientId, Title = medicalCase.Title });
        return medicalCase;
    }

    public CaseMember AddMember(Guid userId, CaseRole role, PermissionLevel permissionLevel)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            throw new DomainException("Kullanıcı zaten vaka üyesi.");
        }

        var member = CaseMember.Create(Id, userId, role, permissionLevel);
        _members.Add(member);
        return member;
    }

    /// <summary>İlk belge Draft → CollectingData geçişini tetikler. Invariant 5: Closed vakaya belge eklenemez.</summary>
    public MedicalDocument AddDocument(string title, Guid uploadedByUserId)
    {
        if (Status == CaseStatus.Closed)
        {
            throw new DomainException("Kapalı vakaya belge eklenemez — önce FollowUp ile yeniden açılmalı.");
        }

        var document = MedicalDocument.Create(Id, title, uploadedByUserId);
        _documents.Add(document);
        Raise(new DocumentUploaded { CaseId = Id, DocumentId = document.Id, DocumentType = document.Type, UploadedByUserId = uploadedByUserId });

        if (Status == CaseStatus.Draft)
        {
            TransitionTo(CaseStatus.CollectingData, "İlk belge yüklendi");
        }

        return document;
    }

    public DicomStudy AddDicomStudy(Modality modality, DateTime studyDateUtc, string? studyInstanceUid = null, string? description = null)
    {
        if (Status == CaseStatus.Closed)
        {
            throw new DomainException("Kapalı vakaya çalışma eklenemez — önce FollowUp ile yeniden açılmalı.");
        }

        var study = DicomStudy.Create(Id, modality, studyDateUtc, studyInstanceUid, description);
        _dicomStudies.Add(study);
        return study;
    }

    public Measurement AddMeasurement(
        string name,
        decimal value,
        MeasurementType type,
        MeasurementMethod method = MeasurementMethod.Manual,
        string? unit = null,
        DateTime? measuredAtUtc = null,
        Guid? studyId = null,
        Guid? seriesId = null)
    {
        var measurement = Measurement.Create(Id, name, value, type, method, unit, measuredAtUtc, studyId, seriesId);
        _measurements.Add(measurement);
        return measurement;
    }

    /// <summary>CollectingData → AIAnalysis: en az bir belge kalite kontrolünden geçmiş olmalı.</summary>
    public void StartAiAnalysis()
    {
        if (!_documents.Any(d => d.Status == DocumentStatus.QualityChecked))
        {
            throw new DomainException("AI analizi için kalite kontrolünden geçmiş en az bir belge gerekli.");
        }

        TransitionTo(CaseStatus.AIAnalysis, "Belge kalite kontrolünden geçti");
    }

    /// <summary>AIAnalysis → DoctorReview: her zaman geçer; düşük confidence yalnızca önceliği yükseltir.</summary>
    public void CompleteAiAnalysis() => TransitionTo(CaseStatus.DoctorReview, "AI analizi tamamlandı");

    /// <summary>DoctorReview → Treatment: tedavi planı doktor onayıyla oluştu.</summary>
    public void BeginTreatment() => TransitionTo(CaseStatus.Treatment, "Tedavi planı oluşturuldu");

    /// <summary>Treatment → FollowUp: kontrol tarihi planlandı.</summary>
    public void ScheduleFollowUp() => TransitionTo(CaseStatus.FollowUp, "Kontrol planlandı");

    /// <summary>FollowUp → CollectingData: yeni belge veya semptom bildirimi.</summary>
    public void ReceiveNewData(string reason) => TransitionTo(CaseStatus.CollectingData, reason);

    /// <summary>FollowUp → Closed: doktor veya admin kapatır.</summary>
    public void Close()
    {
        TransitionTo(CaseStatus.Closed, "Vaka kapatıldı");
        Raise(new CaseClosed { CaseId = Id });
    }

    /// <summary>Closed → FollowUp: geçmiş korunur, Draft'a dönülmez (bkz. state machine dokümanı).</summary>
    public void Reopen(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        TransitionTo(CaseStatus.FollowUp, reason);
        Raise(new CaseReopened { CaseId = Id, Reason = reason });
    }

    public void SetRiskLevel(RiskLevel riskLevel) => RiskLevel = riskLevel;

    private void TransitionTo(CaseStatus toStatus, string? reason)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(toStatus))
        {
            throw new DomainException($"Geçersiz durum geçişi: {Status} → {toStatus}.");
        }

        var fromStatus = Status;
        Status = toStatus;
        Raise(new CaseStatusChanged { CaseId = Id, FromStatus = fromStatus, ToStatus = toStatus, Reason = reason });
    }
}
