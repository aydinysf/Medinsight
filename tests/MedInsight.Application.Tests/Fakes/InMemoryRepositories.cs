using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Tests.Fakes;

public sealed class InMemoryUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public void Add(User user) => Users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Email == email.Trim().ToLowerInvariant()));

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(u => u.Email == email.Trim().ToLowerInvariant()));
}

public sealed class InMemoryPatientRepository : IPatientRepository
{
    public List<Patient> Patients { get; } = [];

    public int SaveCount { get; private set; }

    public void Add(Patient patient) => Patients.Add(patient);

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Patients.FirstOrDefault(p => p.Id == id));

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Patients.Any(p => p.Id == id));

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryCaseRepository : ICaseRepository
{
    public List<Case> Cases { get; } = [];

    public int SaveCount { get; private set; }

    public void Add(Case medicalCase) => Cases.Add(medicalCase);

    public Task<Case?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Cases.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Case>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Case>>(Cases.Where(c => c.PatientId == patientId).ToList());

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
