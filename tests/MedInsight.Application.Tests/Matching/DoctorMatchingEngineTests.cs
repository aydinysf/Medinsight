using MedInsight.Application.Matching;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;
using Microsoft.Extensions.Options;

namespace MedInsight.Application.Tests.Matching;

public class DoctorMatchingEngineTests
{
    private static DoctorMatchingEngine Engine(int maxResults = 5) =>
        new(Options.Create(new MatchingOptions { MaxResults = maxResults }));

    private static Case NeuroCase() => Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Baş ağrısı", BodySystem.Neuro);

    private static Doctor Verified(string specialty, int years = 5, int capacity = 5)
    {
        var doctor = Doctor.Create(Guid.NewGuid(), specialty, $"TR-{Guid.NewGuid():N}", yearsOfExperience: years, capacityThreshold: capacity);
        var verification = doctor.SubmitVerification(VerificationDocumentType.Diploma, "url", null, null);
        doctor.ApproveVerification(verification.Id, Guid.NewGuid());
        return doctor;
    }

    private static IReadOnlyDictionary<Guid, string> Names(params Doctor[] doctors) =>
        doctors.ToDictionary(d => d.Id, d => $"Dr. {d.Specialty}");

    [Fact]
    public void Brans_uyumu_yuksek_agirlik_tasir()
    {
        var neuro = Verified("Nöroloji");
        var cardio = Verified("Kardiyoloji");

        var results = Engine().Match(NeuroCase(), [cardio, neuro], new Dictionary<Guid, ReviewerProfile>(), Names(neuro, cardio));

        Assert.Equal(neuro.Id, results[0].DoctorId);
        Assert.Equal(1m, results[0].ScoreBreakdown["Specialty"]);
        Assert.Equal(0.2m, results[1].ScoreBreakdown["Specialty"]);
    }

    [Fact]
    public void Away_doktor_sonuc_kumesinden_cikarilir()
    {
        var away = Verified("Nöroloji");
        away.SetManualOverride(AvailabilityStatus.Away);
        var available = Verified("Nöroloji");

        var results = Engine().Match(NeuroCase(), [away, available], new Dictionary<Guid, ReviewerProfile>(), Names(away, available));

        Assert.Single(results);
        Assert.Equal(available.Id, results[0].DoctorId);
    }

    [Fact]
    public void Busy_doktor_yogun_etiketiyle_listelenir_ve_secilebilir()
    {
        var busy = Verified("Nöroloji", capacity: 1);
        busy.IncrementActiveCases();

        var results = Engine().Match(NeuroCase(), [busy], new Dictionary<Guid, ReviewerProfile>(), Names(busy));

        var match = Assert.Single(results);
        Assert.Equal("Busy", match.AvailabilityTag);
        Assert.Equal(0.3m, match.ScoreBreakdown["Availability"]);
    }

    [Fact]
    public void En_fazla_MaxResults_oneri_doner()
    {
        var doctors = Enumerable.Range(0, 8).Select(_ => Verified("Nöroloji")).ToArray();

        var results = Engine(maxResults: 5).Match(NeuroCase(), doctors, new Dictionary<Guid, ReviewerProfile>(), Names(doctors));

        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void Skor_dokumu_bes_faktoru_de_icerir_aciklanabilirlik()
    {
        var doctor = Verified("Nöroloji");

        var results = Engine().Match(NeuroCase(), [doctor], new Dictionary<Guid, ReviewerProfile>(), Names(doctor));

        var breakdown = results[0].ScoreBreakdown;
        Assert.Equal(["Availability", "Experience", "Location", "ResponseSpeed", "Specialty"], breakdown.Keys.OrderBy(k => k).ToArray());
    }

    [Fact]
    public void Tecrube_ReviewerProfile_vaka_sayisindan_beslenir()
    {
        var junior = Verified("Nöroloji", years: 0);
        var senior = Verified("Nöroloji", years: 0);
        var seniorProfile = ReviewerProfile.Create(senior.Id, "Nöroloji", 0);
        for (var i = 0; i < 50; i++)
        {
            seniorProfile.RecordReview(corrected: false);
        }

        var results = Engine().Match(
            NeuroCase(),
            [junior, senior],
            new Dictionary<Guid, ReviewerProfile> { [senior.Id] = seniorProfile },
            Names(junior, senior));

        Assert.Equal(senior.Id, results[0].DoctorId);
        Assert.True(results[0].ScoreBreakdown["Experience"] > results[1].ScoreBreakdown["Experience"]);
    }

    [Fact]
    public void Ozel_brans_parametresi_BodySystem_haritasini_gecersiz_kilar()
    {
        var cardio = Verified("Kardiyoloji");

        var results = Engine().Match(NeuroCase(), [cardio], new Dictionary<Guid, ReviewerProfile>(), Names(cardio), requiredSpecialty: "Kardiyoloji");

        Assert.Equal(1m, results[0].ScoreBreakdown["Specialty"]);
    }
}
