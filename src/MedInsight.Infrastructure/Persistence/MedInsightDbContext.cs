using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;
using MedInsight.Infrastructure.Persistence.Outbox;
using MedInsight.TimelineService;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Persistence;

public sealed class MedInsightDbContext(DbContextOptions<MedInsightDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<Caregiver> Caregivers => Set<Caregiver>();

    public DbSet<Case> Cases => Set<Case>();

    public DbSet<CaseMember> CaseMembers => Set<CaseMember>();

    public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();

    public DbSet<DicomStudy> DicomStudies => Set<DicomStudy>();

    public DbSet<DicomSeries> DicomSeries => Set<DicomSeries>();

    public DbSet<Measurement> Measurements => Set<Measurement>();

    public DbSet<TimelineEntry> TimelineEntries => Set<TimelineEntry>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<Storage.IdempotencyRecord> IdempotencyRecords => Set<Storage.IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedInsightDbContext).Assembly);

        // Id'ler her zaman domain'de üretilir (Entity taban sınıfı). ValueGeneratedNever
        // olmadan EF, izlenen aggregate'e sonradan eklenen çocukları "mevcut" sanıp
        // INSERT yerine UPDATE üretir (DbUpdateConcurrencyException).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var id = entityType.FindProperty("Id");
            if (id is not null && id.ClrType == typeof(Guid))
            {
                id.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
