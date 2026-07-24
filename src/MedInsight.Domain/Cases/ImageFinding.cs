using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

/// <summary>
/// Açık kaynak görüntü modelinin ham çıktısı — Case içinde AYRI bir "ek bilgi"
/// bloğu (ADR-010). Yapısal izolasyon: DifferentialDiagnosis'a referans
/// verilemez, confidence eşiği mantığına girmez, disclaimer zorunludur ve
/// arayüzde her zaman "Deneysel — doğrulanmamış" etiketiyle ayrı gösterilir.
/// Source ileride ValidatedImageModel ile genişler (ADR-014 Post-MVP).
/// </summary>
public sealed class ImageFinding : Entity
{
    private ImageFinding()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid? StudyId { get; private set; }

    public string ModelName { get; private set; } = null!;

    public string ModelSource { get; private set; } = null!;

    public string OutputType { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public string RawOutputJson { get; private set; } = null!;

    public string Disclaimer { get; private set; } = null!;

    internal static ImageFinding Create(
        Guid caseId,
        Guid? studyId,
        string modelName,
        string modelSource,
        string outputType,
        string description,
        string rawOutputJson,
        string disclaimer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        // ADR-010: disclaimer kontrattan çıkarılamaz.
        if (string.IsNullOrWhiteSpace(disclaimer))
        {
            throw new DomainException("Doğrulanmamış görüntü modeli bulgusu zorunlu disclaimer olmadan kaydedilemez (ADR-010).");
        }

        return new ImageFinding
        {
            CaseId = caseId,
            StudyId = studyId,
            ModelName = modelName,
            ModelSource = modelSource,
            OutputType = outputType,
            Description = description,
            RawOutputJson = string.IsNullOrWhiteSpace(rawOutputJson) ? "{}" : rawOutputJson,
            Disclaimer = disclaimer,
        };
    }
}
