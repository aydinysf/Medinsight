using MedInsight.Infrastructure.Persistence.Outbox;
using MedInsight.Infrastructure.Storage;
using MedInsight.TimelineService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key).HasMaxLength(200);
        builder.Property(r => r.ResponseJson).HasColumnType("jsonb").IsRequired();
    }
}

public sealed class TimelineEntryConfiguration : IEntityTypeConfiguration<TimelineEntry>
{
    public void Configure(EntityTypeBuilder<TimelineEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EventType).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Summary).HasMaxLength(1000).IsRequired();

        // Zorunlu bileşik indeks (bkz. docs/architecture/timeline-service.md)
        builder.HasIndex(e => new { e.CaseId, e.OccurredAt });
    }
}

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventClrType).HasMaxLength(500).IsRequired();
        builder.Property(m => m.EventType).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2000);

        builder.HasIndex(m => new { m.ProcessedAtUtc, m.OccurredAt });
    }
}
