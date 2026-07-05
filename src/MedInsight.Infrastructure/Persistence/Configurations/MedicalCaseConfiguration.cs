using MedInsight.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class MedicalCaseConfiguration : IEntityTypeConfiguration<MedicalCase>
{
    public void Configure(EntityTypeBuilder<MedicalCase> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(c => c.PatientId);

        builder.HasMany(c => c.Studies)
            .WithOne()
            .HasForeignKey(s => s.MedicalCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documents)
            .WithOne()
            .HasForeignKey(d => d.MedicalCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Measurements)
            .WithOne()
            .HasForeignKey(m => m.MedicalCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.TimelineEvents)
            .WithOne()
            .HasForeignKey(e => e.MedicalCaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
