using MedInsight.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class MedicalDocumentConfiguration : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(d => d.ContentText)
            .HasColumnType("text");

        builder.Property(d => d.DocumentDateUtc)
            .HasColumnType("timestamptz");

        builder.Property(d => d.CreatedAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(d => d.MedicalCaseId);
    }
}
