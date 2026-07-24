using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Identity;
using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Repositories;

public sealed class DoctorRepository(MedInsightDbContext db) : IDoctorRepository
{
    public void Add(Doctor doctor) => db.Doctors.Add(doctor);

    public void AddReviewerProfile(ReviewerProfile profile) => db.ReviewerProfiles.Add(profile);

    public Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Doctors.Include(d => d.Verifications).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<Doctor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Doctors.Include(d => d.Verifications).FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

    public Task<Doctor?> GetByVerificationIdAsync(Guid verificationId, CancellationToken cancellationToken = default) =>
        db.Doctors.Include(d => d.Verifications)
            .FirstOrDefaultAsync(d => d.Verifications.Any(v => v.Id == verificationId), cancellationToken);

    public async Task<IReadOnlyList<Doctor>> GetPendingVerificationAsync(CancellationToken cancellationToken = default) =>
        await db.Doctors.AsNoTracking()
            .Include(d => d.Verifications)
            .Where(d => d.Verifications.Any(v => v.Status == VerificationStatus.Pending))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Doctor>> GetVerifiedAsync(CancellationToken cancellationToken = default) =>
        await db.Doctors.AsNoTracking()
            .Where(d => d.VerificationStatus == VerificationStatus.Verified)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, ReviewerProfile>> GetReviewerProfilesAsync(IReadOnlyCollection<Guid> doctorIds, CancellationToken cancellationToken = default) =>
        await db.ReviewerProfiles.AsNoTracking()
            .Where(p => doctorIds.Contains(p.DoctorId))
            .ToDictionaryAsync(p => p.DoctorId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
