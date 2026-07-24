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
    private readonly List<AiAnalysis> _aiAnalyses = [];
    private readonly List<HealthRouteSnapshot> _healthRouteSnapshots = [];

    private Case()
    {
    }

    public Guid PatientId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public BodySystem BodySystem { get; private set; }

    public CaseStatus Status { get; private set; }

    public RiskLevel RiskLevel { get; private set; }

    public ReviewPriority ReviewPriority { get; private set; }

    public HealthRoute? HealthRoute { get; private set; }

    public IReadOnlyCollection<AiAnalysis> AiAnalyses => _aiAnalyses.AsReadOnly();

    public IReadOnlyCollection<HealthRouteSnapshot> HealthRouteSnapshots => _healthRouteSnapshots.AsReadOnly();

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

        // ADR-002: Version 1 — vaka açıldığında ilk rota snapshot'ı (invariant 1: her zaman tek current).
        medicalCase.UpdateHealthRoute(
            status: "Vaka oluşturuldu",
            nextStep: "Tıbbi belgelerinizi yükleyin",
            riskLevel: RiskLevel.Unknown,
            triggeredBy: RouteTrigger.System,
            triggerSourceId: null,
            reason: "İlk rota");

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
    public MedicalDocument AddDocument(
        string title,
        Guid uploadedByUserId,
        string? storageKey = null,
        string? originalFileName = null,
        string? contentType = null,
        long sizeBytes = 0,
        string? contentHash = null)
    {
        if (Status == CaseStatus.Closed)
        {
            throw new DomainException("Kapalı vakaya belge eklenemez — önce FollowUp ile yeniden açılmalı.");
        }

        var document = MedicalDocument.Create(Id, title, uploadedByUserId, storageKey, originalFileName, contentType, sizeBytes, contentHash);
        _documents.Add(document);
        Raise(new DocumentUploaded { CaseId = Id, DocumentId = document.Id, DocumentType = document.Type, UploadedByUserId = uploadedByUserId });

        if (Status == CaseStatus.Draft)
        {
            TransitionTo(CaseStatus.CollectingData, "İlk belge yüklendi");
        }

        return document;
    }

    /// <summary>Ingestion pipeline 1. aşama: kural tabanlı sınıflandırma sonucu (bkz. ingestion-pipeline.md).</summary>
    public void ClassifyDocument(Guid documentId, DocumentType type)
    {
        var document = GetDocument(documentId);
        document.Classify(type);
        Raise(new DocumentClassified { CaseId = Id, DocumentId = documentId, DocumentType = type });
    }

    /// <summary>Sınıflandırılamayan dosya sessizce yok sayılmaz — hasta yeniden yükleme için bilgilendirilir.</summary>
    public void MarkDocumentClassificationFailed(Guid documentId, string reason)
    {
        var document = GetDocument(documentId);
        document.MarkClassificationFailed();
        Raise(new DocumentClassificationFailed { CaseId = Id, DocumentId = documentId, Reason = reason });
    }

    /// <summary>
    /// Kalite skoru işlenir; yeterliyse belge analiz kuyruğuna hazır olur ve
    /// CollectingData → AIAnalysis geçişi tetiklenir (state machine kuralı).
    /// </summary>
    public void ScoreDocumentQuality(
        Guid documentId,
        decimal overallScore,
        IReadOnlyDictionary<string, decimal> criteriaScores,
        IReadOnlyList<string> failureReasons,
        bool isSufficient)
    {
        var document = GetDocument(documentId);

        if (isSufficient)
        {
            document.MarkQualityChecked();
        }
        else
        {
            document.Reject();
        }

        Raise(new DocumentQualityScored
        {
            CaseId = Id,
            DocumentId = documentId,
            OverallScore = overallScore,
            CriteriaScores = new Dictionary<string, decimal>(criteriaScores),
            FailureReasons = [.. failureReasons],
            IsSufficient = isSufficient,
        });

        if (isSufficient && Status == CaseStatus.CollectingData)
        {
            TransitionTo(CaseStatus.AIAnalysis, "Belge kalite kontrolünden geçti");
            Raise(new AIAnalysisRequested
            {
                CaseId = Id,
                DocumentIds = _documents.Where(d => d.Status == DocumentStatus.QualityChecked).Select(d => d.Id).ToList(),
            });
        }
    }

    /// <summary>Toplu yüklemedeki her DICOM dosyası: study/series bul-veya-oluştur (bkz. ingestion-pipeline.md).</summary>
    public DicomSeries RegisterDicomFile(
        string studyInstanceUid,
        string seriesInstanceUid,
        Modality modality,
        DateTime? studyDateUtc = null,
        int? seriesNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(studyInstanceUid);
        ArgumentException.ThrowIfNullOrWhiteSpace(seriesInstanceUid);

        var study = _dicomStudies.FirstOrDefault(s => s.StudyInstanceUid == studyInstanceUid);
        if (study is null)
        {
            study = DicomStudy.Create(Id, modality, studyDateUtc ?? DateTime.UtcNow, studyInstanceUid);
            _dicomStudies.Add(study);
        }

        return study.RegisterFile(seriesInstanceUid, modality, seriesNumber);
    }

    /// <summary>Bekleme penceresi kapandı: grup tamamlandı sayılır, DICOMStudyGrouped yayınlanır.</summary>
    public void CompleteDicomGrouping(Guid studyId)
    {
        var study = _dicomStudies.FirstOrDefault(s => s.Id == studyId)
            ?? throw new DomainException("Çalışma bu vakada bulunamadı.");

        study.MarkGrouped();
        Raise(new DicomStudyGrouped
        {
            CaseId = Id,
            StudyId = studyId,
            SeriesList = study.Series
                .Select(s => new GroupedSeriesInfo { SeriesId = s.Id, Modality = s.Modality, SliceCount = s.SliceCount })
                .ToList(),
        });
    }

    /// <summary>Kaliteden geçen belge için işleme yolu kararı — denetlenebilir kayıt (RoutingDecided).</summary>
    public DocumentRoute DecideRouting(Guid documentId)
    {
        var document = GetDocument(documentId);
        if (document.Status != DocumentStatus.QualityChecked)
        {
            throw new DomainException("Yönlendirme yalnızca kalite kontrolünden geçmiş belge için yapılabilir.");
        }

        var route = document.Type switch
        {
            DocumentType.TextualReport or DocumentType.ScannedReport => DocumentRoute.TextExtraction,
            DocumentType.DicomFile => DocumentRoute.RadiologyInference,
            _ => DocumentRoute.StorageOnly,
        };

        Raise(new RoutingDecided { CaseId = Id, DocumentId = documentId, Route = route });
        return route;
    }

    public void StoreExtractedText(Guid documentId, string text, decimal? ocrConfidence = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        GetDocument(documentId).SetExtractedText(text, ocrConfidence);
    }

    /// <summary>
    /// AI ön analizi kaydeder ve AIAnalysis → DoctorReview geçişini tetikler
    /// (state machine: her zaman geçer, düşük confidence yalnız önceliği etkiler).
    /// </summary>
    public AiAnalysis AddAiAnalysis(
        string modelVersion,
        string promptVersion,
        decimal confidenceScore,
        string summary,
        string patientMessage,
        IReadOnlyList<AiFindingInput> findings,
        IReadOnlyList<DifferentialDiagnosisInput> differentialDiagnoses)
    {
        if (Status != CaseStatus.AIAnalysis)
        {
            throw new DomainException("AI analizi yalnızca AIAnalysis durumundaki vakaya eklenebilir.");
        }

        var analysis = AiAnalysis.Create(Id, modelVersion, promptVersion, confidenceScore, summary, patientMessage);

        var createdFindings = new List<AiFinding>();
        foreach (var input in findings)
        {
            createdFindings.Add(analysis.AddFinding(input.Description, input.Source, input.SourceDocumentId, input.Disclaimer));
        }

        foreach (var input in differentialDiagnoses)
        {
            var sourceIds = input.SourceFindingIndexes.Select(i =>
            {
                if (i < 0 || i >= createdFindings.Count)
                {
                    throw new DomainException("DifferentialDiagnosis geçersiz bulgu indeksine referans veriyor.");
                }

                return createdFindings[i].Id;
            }).ToList();

            analysis.AddDifferentialDiagnosis(input.Name, input.ConfidenceScore, input.RiskLevel, sourceIds);
        }

        _aiAnalyses.Add(analysis);

        Raise(new AIAnalysisCompleted
        {
            CaseId = Id,
            AnalysisId = analysis.Id,
            ModelVersion = modelVersion,
            PromptVersion = promptVersion,
            ConfidenceScore = confidenceScore,
            FindingIds = createdFindings.Select(f => f.Id).ToList(),
        });

        CompleteAiAnalysis();
        return analysis;
    }

    /// <summary>ADR-004: düşük confidence dalı — doktor inceleme önceliğini yükseltir.</summary>
    public void EscalateReviewPriority(Guid analysisId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ReviewPriority = ReviewPriority.High;
        Raise(new DoctorReviewPriorityRaised { CaseId = Id, AnalysisId = analysisId, Reason = reason });
    }

    /// <summary>
    /// Yeni rota snapshot'ı (ADR-002): append-only zincir, doğrusal geçmiş,
    /// current her zaman tek. Hiçbir snapshot güncellenmez/silinmez.
    /// </summary>
    public HealthRouteSnapshot UpdateHealthRoute(
        string status,
        string nextStep,
        RiskLevel riskLevel,
        RouteTrigger triggeredBy,
        Guid? triggerSourceId,
        string reason)
    {
        var previous = _healthRouteSnapshots.OrderByDescending(s => s.VersionNumber).FirstOrDefault();
        var snapshot = HealthRouteSnapshot.Create(
            Id,
            previous?.Id,
            (previous?.VersionNumber ?? 0) + 1,
            status,
            nextStep,
            riskLevel,
            triggeredBy,
            triggerSourceId,
            reason);

        _healthRouteSnapshots.Add(snapshot);

        if (HealthRoute is null)
        {
            HealthRoute = HealthRoute.Create(Id, snapshot);
        }
        else
        {
            HealthRoute.MoveTo(snapshot);
        }

        RiskLevel = riskLevel;

        Raise(new HealthRouteSnapshotCreated
        {
            CaseId = Id,
            SnapshotId = snapshot.Id,
            PreviousVersionId = snapshot.PreviousVersionId,
            VersionNumber = snapshot.VersionNumber,
            Status = status,
            NextStep = nextStep,
            RiskLevel = riskLevel,
            TriggeredBy = triggeredBy,
            TriggerSourceId = triggerSourceId,
            Reason = reason,
        });

        return snapshot;
    }

    private MedicalDocument GetDocument(Guid documentId) =>
        _documents.FirstOrDefault(d => d.Id == documentId)
            ?? throw new DomainException("Belge bu vakada bulunamadı.");

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

public enum ReviewPriority
{
    Normal = 0,
    High = 1,
}

/// <summary>AddAiAnalysis girdileri — bulgu Id'leri aggregate içinde üretildiği için indeksle referans verilir.</summary>
public sealed record AiFindingInput(string Description, AiFindingSource Source, Guid? SourceDocumentId, string? Disclaimer = null);

public sealed record DifferentialDiagnosisInput(string Name, decimal ConfidenceScore, RiskLevel RiskLevel, IReadOnlyList<int> SourceFindingIndexes);
