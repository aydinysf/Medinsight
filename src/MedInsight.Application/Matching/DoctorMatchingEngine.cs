using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;
using Microsoft.Extensions.Options;

namespace MedInsight.Application.Matching;

public sealed class MatchingOptions
{
    public const string SectionName = "Matching";

    public int MaxResults { get; set; } = 5;

    /// <summary>Faktör ağırlıkları — kod değişikliği olmadan güncellenebilir (ADR-003).</summary>
    public Dictionary<string, decimal> Weights { get; set; } = new()
    {
        ["Specialty"] = 5m,      // yüksek
        ["Location"] = 3m,       // orta (MVP: konum verisi yok — nötr skor)
        ["Availability"] = 3m,   // orta
        ["Experience"] = 1m,     // düşük
        ["ResponseSpeed"] = 1m,  // düşük
    };
}

/// <summary>Skor dökümü açıklanabilirlik ilkesi gereği her faktörü ayrı gösterir.</summary>
public sealed record DoctorMatchResultDto(
    Guid DoctorId,
    string FullName,
    string? Title,
    string Specialty,
    decimal Score,
    IReadOnlyDictionary<string, decimal> ScoreBreakdown,
    string AvailabilityTag);

/// <summary>
/// 5 faktörlü, konfigüre edilebilir skorlama (ADR-003). Bu motor ATAMA YAPMAZ —
/// yalnızca önerir; nihai seçim hasta/caregiver'ındır (regülasyon sınırı).
/// </summary>
public sealed class DoctorMatchingEngine(IOptions<MatchingOptions> options)
{
    private static readonly IReadOnlyDictionary<BodySystem, string> SpecialtyMap = new Dictionary<BodySystem, string>
    {
        [BodySystem.Neuro] = "Nöroloji",
        [BodySystem.Cardio] = "Kardiyoloji",
        [BodySystem.Oncology] = "Onkoloji",
        [BodySystem.Endocrine] = "Endokrinoloji",
        [BodySystem.Orthopedic] = "Ortopedi",
    };

    public IReadOnlyList<DoctorMatchResultDto> Match(
        Case medicalCase,
        IReadOnlyList<Doctor> verifiedDoctors,
        IReadOnlyDictionary<Guid, ReviewerProfile> profiles,
        IReadOnlyDictionary<Guid, string> doctorNames,
        string? requiredSpecialty = null)
    {
        var targetSpecialty = requiredSpecialty ?? SpecialtyMap.GetValueOrDefault(medicalCase.BodySystem);
        var weights = options.Value.Weights;

        var results = new List<DoctorMatchResultDto>();
        foreach (var doctor in verifiedDoctors)
        {
            // Away doktorlar sonuç kümesinden tamamen çıkarılır (ADR-009).
            var availability = doctor.EffectiveStatus;
            if (availability == AvailabilityStatus.Away)
            {
                continue;
            }

            var profile = profiles.GetValueOrDefault(doctor.Id);

            var breakdown = new Dictionary<string, decimal>
            {
                ["Specialty"] = SpecialtyScore(doctor.Specialty, targetSpecialty),
                ["Location"] = 0.5m, // MVP: konum verisi yok — nötr
                ["Availability"] = availability == AvailabilityStatus.Available ? 1m : 0.3m,
                ["Experience"] = ExperienceScore(doctor, profile),
                ["ResponseSpeed"] = ResponseSpeedScore(profile),
            };

            var totalWeight = breakdown.Keys.Sum(k => weights.GetValueOrDefault(k, 1m));
            var score = totalWeight == 0
                ? 0
                : Math.Round(breakdown.Sum(kv => kv.Value * weights.GetValueOrDefault(kv.Key, 1m)) / totalWeight, 4);

            results.Add(new DoctorMatchResultDto(
                doctor.Id,
                doctorNames.GetValueOrDefault(doctor.Id, "?"),
                doctor.Title,
                doctor.Specialty,
                score,
                breakdown,
                availability.ToString()));
        }

        return results
            .OrderByDescending(r => r.Score)
            .Take(options.Value.MaxResults)
            .ToList();
    }

    private static decimal SpecialtyScore(string doctorSpecialty, string? targetSpecialty)
    {
        if (targetSpecialty is null)
        {
            return 0.5m; // hedef branş bilinmiyor — nötr
        }

        return doctorSpecialty.Contains(targetSpecialty, StringComparison.OrdinalIgnoreCase)
            || targetSpecialty.Contains(doctorSpecialty, StringComparison.OrdinalIgnoreCase)
                ? 1m
                : 0.2m;
    }

    private static decimal ExperienceScore(Doctor doctor, ReviewerProfile? profile)
    {
        var reviewComponent = profile is null ? 0m : Math.Min(profile.CaseReviewCount / 50m, 1m);
        var yearsComponent = Math.Min(doctor.YearsOfExperience / 20m, 1m);
        return Math.Round((reviewComponent + yearsComponent) / 2, 4);
    }

    private static decimal ResponseSpeedScore(ReviewerProfile? profile) =>
        profile?.AverageResponseTimeMinutes is null
            ? 0.5m // geçmiş veri yok — nötr
            : Math.Max(0m, Math.Min(1m, 1m - (profile.AverageResponseTimeMinutes.Value / (24 * 60m))));
}
