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
        builder.HasMany(c => c.Consultations).WithOne().HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Treatments).WithOne().HasForeignKey(t => t.CaseId).OnDelete(DeleteBehavior.Cascade);
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

public sealed class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.CaseId, c.Status });
        builder.HasIndex(c => c.DoctorId);

        builder.HasMany(c => c.Messages).WithOne().HasForeignKey(m => m.ConsultationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.ClinicalNotes).WithOne().HasForeignKey(n => n.ConsultationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ConsultationMessageConfiguration : IEntityTypeConfiguration<ConsultationMessage>
{
    public void Configure(EntityTypeBuilder<ConsultationMessage> builder)
    {
        builder.HasKey(m => m.Id);
        // TODO(security): at-rest column-level şifreleme (security-architecture.md) — MVP teknik borcu.
        builder.Property(m => m.Content).HasMaxLength(4000).IsRequired();
        builder.HasIndex(m => new { m.ConsultationId, m.CreatedAtUtc });
    }
}

public sealed class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Content).HasMaxLength(8000).IsRequired();
        builder.HasIndex(n => n.ConsultationId);
    }
}

public sealed class TreatmentConfiguration : IEntityTypeConfiguration<Treatment>
{
    public void Configure(EntityTypeBuilder<Treatment> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Description).HasMaxLength(8000).IsRequired();
        builder.Property(t => t.FollowUpDate).HasColumnType("date");
        builder.HasIndex(t => t.CaseId);
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
