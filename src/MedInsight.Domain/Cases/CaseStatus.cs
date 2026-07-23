namespace MedInsight.Domain.Cases;

/// <summary>Bkz. docs/domain/case-lifecycle-state-machine.md</summary>
public enum CaseStatus
{
    Draft = 0,
    CollectingData = 1,
    AIAnalysis = 2,
    DoctorReview = 3,
    Treatment = 4,
    FollowUp = 5,
    Closed = 6,
}
