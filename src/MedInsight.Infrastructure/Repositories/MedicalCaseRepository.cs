using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Entities;
using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Repositories;

public sealed class MedicalCaseRepository(MedInsightDbContext db) : IMedicalCaseRepository
{
    public void Add(MedicalCase medicalCase) => db.MedicalCases.Add(medicalCase);

    public Task<MedicalCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.MedicalCases.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MedicalCase>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        await db.MedicalCases.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
