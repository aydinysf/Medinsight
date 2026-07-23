using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Identity;
using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Repositories;

public sealed class UserRepository(MedInsightDbContext db) : IUserRepository
{
    public void Add(User user) => db.Users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().AnyAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);
}

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
