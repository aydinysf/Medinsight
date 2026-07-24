using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Doctors;

/// <summary>Override = null → sistem hesabına (ComputedStatus) dönülür (ADR-009).</summary>
public sealed record SetAvailability(AvailabilityStatus? Override, DateTime? OverrideExpiresAt);

public sealed record AvailabilityDto(
    AvailabilityStatus EffectiveStatus,
    AvailabilityStatus ComputedStatus,
    AvailabilityStatus? ManualOverride,
    DateTime? OverrideExpiresAt,
    int ActiveCaseCount,
    int CapacityThreshold);

public sealed class SetAvailabilityHandler(IDoctorRepository doctors, ICurrentUser currentUser)
{
    public async Task<AvailabilityDto> HandleAsync(SetAvailability command, CancellationToken cancellationToken = default)
    {
        var doctor = await doctors.GetByUserIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenAccessException("Doktor profili bulunamadı.");

        doctor.SetManualOverride(command.Override, command.OverrideExpiresAt);
        await doctors.SaveChangesAsync(cancellationToken);

        return new AvailabilityDto(
            doctor.EffectiveStatus,
            doctor.ComputedStatus,
            doctor.ManualOverride,
            doctor.OverrideExpiresAt,
            doctor.ActiveCaseCount,
            doctor.CapacityThreshold);
    }
}
