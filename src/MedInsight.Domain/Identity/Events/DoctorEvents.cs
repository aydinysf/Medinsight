using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity.Events;

public sealed record DoctorVerificationSubmitted : DomainEvent
{
    public required Guid VerificationId { get; init; }

    public required Guid DoctorId { get; init; }

    public required VerificationDocumentType DocumentType { get; init; }
}

public sealed record DoctorVerified : DomainEvent
{
    public required Guid VerificationId { get; init; }

    public required Guid DoctorId { get; init; }

    public required Guid VerifiedByAdminId { get; init; }
}

public sealed record DoctorVerificationRejected : DomainEvent
{
    public required Guid VerificationId { get; init; }

    public required Guid DoctorId { get; init; }

    public required Guid VerifiedByAdminId { get; init; }

    public required string RejectionReason { get; init; }
}

public sealed record DoctorAvailabilityChanged : DomainEvent
{
    public required Guid DoctorId { get; init; }

    public required AvailabilityStatus PreviousStatus { get; init; }

    public required AvailabilityStatus NewStatus { get; init; }

    public required string ChangedBy { get; init; }

    public DateTime? OverrideExpiresAt { get; init; }
}
