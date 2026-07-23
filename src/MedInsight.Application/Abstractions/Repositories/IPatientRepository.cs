using MedInsight.Domain.Identity;

namespace MedInsight.Application.Abstractions.Repositories;

public interface IPatientRepository
{
    void Add(Patient patient);

    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
