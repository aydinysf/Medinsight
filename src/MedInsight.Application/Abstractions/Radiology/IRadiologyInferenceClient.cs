namespace MedInsight.Application.Abstractions.Radiology;

public sealed record RadiologyFinding(
    string FindingId,
    string ModelName,
    string ModelSource,
    string OutputType,
    string Description,
    string RawOutputJson,
    string Disclaimer);

/// <summary>
/// Python Radiology Inference Service istemcisi (ADR-010). Servis yapılandırılmamışsa
/// null implementasyon kullanılır — pipeline kırılmaz, bulgu üretilmez.
/// </summary>
public interface IRadiologyInferenceClient
{
    bool IsEnabled { get; }

    Task<IReadOnlyList<RadiologyFinding>> AnalyzeStudyAsync(Guid studyId, IReadOnlyList<string> dicomUrls, CancellationToken cancellationToken = default);
}
