using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Common;
using MedInsight.Domain.Identity;
using MedInsight.Domain.Identity.Events;

namespace MedInsight.Application.Doctors;

/// <summary>Doğrulanan doktor için ReviewerProfile açılır (bkz. reviewer-profile.md).</summary>
public sealed class OnDoctorVerifiedCreateReviewerProfile(IDoctorRepository doctors) : IDomainEventHandler<DoctorVerified>
{
    public async Task HandleAsync(DoctorVerified e, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(e.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return;
        }

        var existing = await doctors.GetReviewerProfilesAsync([e.DoctorId], cancellationToken);
        if (existing.ContainsKey(e.DoctorId))
        {
            return; // idempotency
        }

        doctors.AddReviewerProfile(ReviewerProfile.Create(doctor.Id, doctor.Specialty, doctor.YearsOfExperience));
        await doctors.SaveChangesAsync(cancellationToken);
    }
}
