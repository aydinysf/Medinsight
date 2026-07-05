using MedInsight.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Value)
            .HasColumnType("numeric(18,4)");

        builder.Property(m => m.Unit)
            .HasMaxLength(50);

        builder.Property(m => m.MeasuredAtUtc)
            .HasColumnType("timestamptz");

        builder.Property(m => m.CreatedAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(m => m.MedicalCaseId);
    }
}
