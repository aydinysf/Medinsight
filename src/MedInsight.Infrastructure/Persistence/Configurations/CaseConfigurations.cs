using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Ignore(c => c.DomainEvents);

        builder.HasOne<Patient>().WithMany().HasForeignKey(c => c.PatientId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => c.PatientId);

        builder.HasMany(c => c.Members).WithOne().HasForeignKey(m => m.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Documents).WithOne().HasForeignKey(d => d.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.DicomStudies).WithOne().HasForeignKey(s => s.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Measurements).WithOne().HasForeignKey(m => m.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.AiAnalyses).WithOne().HasForeignKey(a => a.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.HealthRouteSnapshots).WithOne().HasForeignKey(s => s.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.HealthRoute).WithOne().HasForeignKey<HealthRoute>(r => r.CaseId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CaseMemberConfiguration : IEntityTypeConfiguration<CaseMember>
{
    public void Configure(EntityTypeBuilder<CaseMember> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasOne<User>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => new { m.CaseId, m.UserId }).IsUnique();
    }
}

public sealed class MedicalDocumentConfiguration : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).HasMaxLength(300).IsRequired();
        builder.HasIndex(d => d.CaseId);
    }
}

public sealed class DicomStudyConfiguration : IEntityTypeConfiguration<DicomStudy>
{
    public void Configure(EntityTypeBuilder<DicomStudy> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StudyInstanceUid).HasMaxLength(128);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.HasIndex(s => new { s.CaseId, s.StudyDateUtc });

        builder.HasMany(s => s.Series).WithOne().HasForeignKey(x => x.StudyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class DicomSeriesConfiguration : IEntityTypeConfiguration<DicomSeries>
{
    public void Configure(EntityTypeBuilder<DicomSeries> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SeriesInstanceUid).HasMaxLength(128);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.HasIndex(s => s.StudyId);
    }
}

public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Value).HasColumnType("numeric(18,4)");
        builder.Property(m => m.Unit).HasMaxLength(50);
        builder.HasIndex(m => m.CaseId);

        builder.HasOne<DicomStudy>().WithMany().HasForeignKey(m => m.StudyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<DicomSeries>().WithMany().HasForeignKey(m => m.SeriesId).OnDelete(DeleteBehavior.SetNull);
    }
}
