using MedInsight.Domain.Identity;

namespace MedInsight.Application.Abstractions.Repositories;

public interface IDoctorRepository
{
    void Add(Doctor doctor);

    void AddReviewerProfile(ReviewerProfile profile);

    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Doctor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Doctor?> GetByVerificationIdAsync(Guid verificationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetPendingVerificationAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetVerifiedAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ReviewerProfile>> GetReviewerProfilesAsync(IReadOnlyCollection<Guid> doctorIds, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
