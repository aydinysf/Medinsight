using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Cases;
using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Repositories;

public sealed class CaseRepository(MedInsightDbContext db) : ICaseRepository
{
    public void Add(Case medicalCase) => db.Cases.Add(medicalCase);

    public Task<Case?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Cases
            .Include(c => c.Members)
            .Include(c => c.Documents)
            .Include(c => c.DicomStudies).ThenInclude(s => s.Series)
            .Include(c => c.Measurements)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Case>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        await db.Cases.AsNoTracking()
            .Include(c => c.Members)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
