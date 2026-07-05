using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Entities;
using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Repositories;

public sealed class PatientRepository(MedInsightDbContext db) : IPatientRepository
{
    public void Add(Patient patient) => db.Patients.Add(patient);

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Patients.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
