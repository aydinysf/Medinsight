using MedInsight.Domain.Entities;

namespace MedInsight.Application.Abstractions.Repositories;

public interface IMedicalCaseRepository
{
    void Add(MedicalCase medicalCase);

    Task<MedicalCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicalCase>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
