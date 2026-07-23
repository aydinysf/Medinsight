using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases.Events;

public sealed record CaseCreated : DomainEvent
{
    public required Guid PatientId { get; init; }

    public required string Title { get; init; }
}

public sealed record CaseStatusChanged : DomainEvent
{
    public required CaseStatus FromStatus { get; init; }

    public required CaseStatus ToStatus { get; init; }

    public string? Reason { get; init; }
}

public sealed record CaseClosed : DomainEvent;

public sealed record CaseReopened : DomainEvent
{
    public required string Reason { get; init; }
}

public sealed record DocumentUploaded : DomainEvent
{
    public required Guid DocumentId { get; init; }

    public required DocumentType DocumentType { get; init; }

    public required Guid UploadedByUserId { get; init; }
}
