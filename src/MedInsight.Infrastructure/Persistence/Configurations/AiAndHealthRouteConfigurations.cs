using MedInsight.Domain.Cases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class AiAnalysisConfiguration : IEntityTypeConfiguration<AiAnalysis>
{
    public void Configure(EntityTypeBuilder<AiAnalysis> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ModelVersion).HasMaxLength(100).IsRequired();
        builder.Property(a => a.PromptVersion).HasMaxLength(100).IsRequired();
        builder.Property(a => a.ConfidenceScore).HasColumnType("numeric(5,4)");
        builder.Property(a => a.Summary).HasMaxLength(8000).IsRequired();
        builder.Property(a => a.PatientMessage).HasMaxLength(4000).IsRequired();
        builder.HasIndex(a => a.CaseId);

        builder.HasMany(a => a.Findings).WithOne().HasForeignKey(f => f.AnalysisId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.DifferentialDiagnoses).WithOne().HasForeignKey(d => d.AnalysisId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AiFindingConfiguration : IEntityTypeConfiguration<AiFinding>
{
    public void Configure(EntityTypeBuilder<AiFinding> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Description).HasMaxLength(4000).IsRequired();
        builder.Property(f => f.Disclaimer).HasMaxLength(1000);
        builder.HasIndex(f => f.CaseId);
    }
}

public sealed class DifferentialDiagnosisConfiguration : IEntityTypeConfiguration<DifferentialDiagnosis>
{
    public void Configure(EntityTypeBuilder<DifferentialDiagnosis> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(500).IsRequired();
        builder.Property(d => d.ConfidenceScore).HasColumnType("numeric(5,4)");
        builder.HasIndex(d => d.CaseId);
    }
}

public sealed class HealthRouteConfiguration : IEntityTypeConfiguration<HealthRoute>
{
    public void Configure(EntityTypeBuilder<HealthRoute> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.CurrentStatus).HasMaxLength(500).IsRequired();
        builder.Property(r => r.NextStep).HasMaxLength(500).IsRequired();
        builder.HasIndex(r => r.CaseId).IsUnique();
    }
}

public sealed class HealthRouteSnapshotConfiguration : IEntityTypeConfiguration<HealthRouteSnapshot>
{
    public void Configure(EntityTypeBuilder<HealthRouteSnapshot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).HasMaxLength(500).IsRequired();
        builder.Property(s => s.NextStep).HasMaxLength(500).IsRequired();
        builder.Property(s => s.Reason).HasMaxLength(1000).IsRequired();

        // Append-only zincir sorguları için kritik bileşik indeks (erd-ai-clinical.md)
        builder.HasIndex(s => new { s.CaseId, s.CreatedAtUtc });
    }
}
