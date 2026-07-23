using MedInsight.Domain.Cases;

namespace MedInsight.Application.Abstractions.Repositories;

public interface ICaseRepository
{
    void Add(Case medicalCase);

    /// <summary>Aggregate operasyonları için izlenen (tracked) nesne döner.</summary>
    Task<Case?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Case>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
