using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;

namespace MedInsight.Application.Doctors;

public sealed record DoctorMeDto(
    DoctorDto Profile,
    AvailabilityDto Availability,
    IReadOnlyList<VerificationDto> Verifications);

/// <summary>Oturumdaki doktorun profili + müsaitlik + doğrulama geçmişi (doktor paneli girişi).</summary>
public sealed class GetMyDoctorProfileQueryHandler(IDoctorRepository doctors, IUserRepository users, ICurrentUser currentUser)
{
    public async Task<DoctorMeDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var doctor = await doctors.GetByUserIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenAccessException("Doktor profili bulunamadı.");
        var user = await users.GetByIdAsync(doctor.UserId, cancellationToken);

        return new DoctorMeDto(
            doctor.ToDto(user?.FullName ?? string.Empty),
            new AvailabilityDto(
                doctor.EffectiveStatus,
                doctor.ComputedStatus,
                doctor.ManualOverride,
                doctor.OverrideExpiresAt,
                doctor.ActiveCaseCount,
                doctor.CapacityThreshold),
            doctor.Verifications.OrderByDescending(v => v.CreatedAtUtc).Select(v => v.ToDto()).ToList());
    }
}

public sealed record DoctorQueueItemDto(
    CaseDto Case,
    ReviewPriority ReviewPriority,
    Guid ConsultationId,
    ConsultationStatus ConsultationStatus,
    DateTime ConsultationStartedAtUtc);

/// <summary>
/// İnceleme kuyruğu: doktorun konsültasyonu olan vakalar, ReviewPriority (ADR-004
/// eskalasyonları öne) ve konsültasyon tarihine göre sıralı.
/// </summary>
public sealed class GetDoctorReviewQueueQueryHandler(ICaseRepository cases, IDoctorRepository doctors, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<DoctorQueueItemDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var doctor = await doctors.GetByUserIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenAccessException("Doktor profili bulunamadı.");

        var doctorCases = await cases.GetByDoctorIdAsync(doctor.Id, cancellationToken);

        return doctorCases
            .Select(c => (Case: c, Consultation: c.Consultations
                .Where(x => x.DoctorId == doctor.Id)
                .OrderByDescending(x => x.StartedAtUtc)
                .First()))
            .OrderByDescending(t => t.Consultation.Status == ConsultationStatus.Active)
            .ThenByDescending(t => t.Case.ReviewPriority)
            .ThenByDescending(t => t.Consultation.StartedAtUtc)
            .Select(t => new DoctorQueueItemDto(
                t.Case.ToDto(),
                t.Case.ReviewPriority,
                t.Consultation.Id,
                t.Consultation.Status,
                t.Consultation.StartedAtUtc))
            .ToList();
    }
}
