using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity;

/// <summary>
/// Doktorun AI onay/düzeltmelerinin "ağırlığı" — Doctor tablosundan ayrı çünkü
/// sık güncellenir (bkz. docs/domain/reviewer-profile.md). MVP'de yalnız iç kullanım.
/// </summary>
public sealed class ReviewerProfile : Entity
{
    private ReviewerProfile()
    {
    }

    public Guid DoctorId { get; private set; }

    public string Specialty { get; private set; } = null!;

    public int YearsOfExperience { get; private set; }

    public int CaseReviewCount { get; private set; }

    public decimal? AverageResponseTimeMinutes { get; private set; }

    /// <summary>AI önerilerini düzeltme sıklığı (0–1) — Learning Loop girdisi.</summary>
    public decimal CorrectionRate { get; private set; }

    private int _correctionCount;

    public static ReviewerProfile Create(Guid doctorId, string specialty, int yearsOfExperience)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(specialty);

        return new ReviewerProfile
        {
            DoctorId = doctorId,
            Specialty = specialty,
            YearsOfExperience = yearsOfExperience,
        };
    }

    public void RecordReview(bool corrected)
    {
        CaseReviewCount++;
        if (corrected)
        {
            _correctionCount++;
        }

        CorrectionRate = Math.Round((decimal)_correctionCount / CaseReviewCount, 4);
    }
}
