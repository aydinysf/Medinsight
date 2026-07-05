using MedInsight.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Persistence;

public sealed class MedInsightDbContext(DbContextOptions<MedInsightDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<MedicalCase> MedicalCases => Set<MedicalCase>();

    public DbSet<Study> Studies => Set<Study>();

    public DbSet<Series> Series => Set<Series>();

    public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();

    public DbSet<Measurement> Measurements => Set<Measurement>();

    public DbSet<TimelineEvent> TimelineEvents => Set<TimelineEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedInsightDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
