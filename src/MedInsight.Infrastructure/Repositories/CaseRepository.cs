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
            .Include(c => c.AiAnalyses).ThenInclude(a => a.Findings)
            .Include(c => c.AiAnalyses).ThenInclude(a => a.DifferentialDiagnoses)
            .Include(c => c.HealthRoute)
            .Include(c => c.HealthRouteSnapshots)
            .Include(c => c.Consultations)
            .Include(c => c.Treatments)
            .Include(c => c.ImageFindings)
            .Include(c => c.HizirChatMessages)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ConsultationMessage>> GetConsultationMessagesAsync(Guid consultationId, CancellationToken cancellationToken = default) =>
        await db.Set<ConsultationMessage>().AsNoTracking()
            .Where(m => m.ConsultationId == consultationId)
            .OrderBy(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Case>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        await db.Cases.AsNoTracking()
            .Include(c => c.Members)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Case>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default) =>
        await db.Cases.AsNoTracking()
            .Include(c => c.Consultations)
            .Where(c => c.Consultations.Any(x => x.DoctorId == doctorId))
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
