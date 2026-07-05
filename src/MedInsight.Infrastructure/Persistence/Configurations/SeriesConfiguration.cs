using MedInsight.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(s => s.StudyId);

        builder.HasMany(s => s.Measurements)
            .WithOne()
            .HasForeignKey(m => m.SeriesId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
