using MedInsight.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class StudyConfiguration : IEntityTypeConfiguration<Study>
{
    public void Configure(EntityTypeBuilder<Study> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.StudyDateUtc)
            .HasColumnType("timestamptz");

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(s => new { s.MedicalCaseId, s.StudyDateUtc });

        builder.HasMany(s => s.Series)
            .WithOne()
            .HasForeignKey(x => x.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Documents)
            .WithOne()
            .HasForeignKey(d => d.StudyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Measurements)
            .WithOne()
            .HasForeignKey(m => m.StudyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
