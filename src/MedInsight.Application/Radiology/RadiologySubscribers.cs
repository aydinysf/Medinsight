using MedInsight.Application.Abstractions.Radiology;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Application.Radiology;

/// <summary>
/// Gruplanmış DICOM çalışması → Radiology Inference Service (ADR-010).
/// Servis kapalıysa sessizce atlanır — çekirdek akış görüntü analizine bağımlı değildir.
/// </summary>
public sealed class OnDicomStudyGroupedRunInference(
    ICaseRepository cases,
    IObjectStorage storage,
    IRadiologyInferenceClient radiology) : IDomainEventHandler<DicomStudyGrouped>
{
    public async Task HandleAsync(DicomStudyGrouped e, CancellationToken cancellationToken)
    {
        if (!radiology.IsEnabled)
        {
            return;
        }

        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null || medicalCase.ImageFindings.Any(f => f.StudyId == e.StudyId))
        {
            return; // idempotency: bu çalışma için bulgular zaten alınmış
        }

        var dicomDocuments = medicalCase.Documents
            .Where(d => d.Type == DocumentType.DicomFile && d.StorageKey is not null && d.Status != DocumentStatus.Rejected)
            .ToList();
        if (dicomDocuments.Count == 0)
        {
            return;
        }

        var urls = new List<string>(dicomDocuments.Count);
        foreach (var document in dicomDocuments)
        {
            urls.Add(await storage.GetPresignedReadUrlAsync(document.StorageKey!, TimeSpan.FromMinutes(15), cancellationToken));
        }

        var findings = await radiology.AnalyzeStudyAsync(e.StudyId, urls, cancellationToken);
        foreach (var finding in findings)
        {
            medicalCase.AddImageFinding(
                e.StudyId,
                finding.ModelName,
                finding.ModelSource,
                finding.OutputType,
                finding.Description,
                finding.RawOutputJson,
                finding.Disclaimer);
        }

        if (findings.Count > 0)
        {
            await cases.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// ADR-014 otomatik koşulun ikinci bacağı: görüntü bulgusu, yüksek riskli tanı
/// adayından SONRA gelirse de escalation önerilir.
/// </summary>
public sealed class OnImageFindingAddedEscalationCheck(ICaseRepository cases) : IDomainEventHandler<ImageFindingAdded>
{
    public async Task HandleAsync(ImageFindingAdded e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null || medicalCase.ReviewPriority == ReviewPriority.High)
        {
            return;
        }

        var hasHighRiskDifferential = medicalCase.AiAnalyses
            .SelectMany(a => a.DifferentialDiagnoses)
            .Any(d => d.RiskLevel >= RiskLevel.High);
        if (!hasHighRiskDifferential)
        {
            return;
        }

        medicalCase.SuggestEscalation(EscalationReason.HighRiskWithUnvalidatedFinding);
        await cases.SaveChangesAsync(cancellationToken);
    }
}
