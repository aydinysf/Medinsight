using MedInsight.Infrastructure.Audit;
using MedInsight.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(200).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.MetadataJson).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(a => new { a.EntityId, a.OccurredAtUtc });
        builder.HasIndex(a => a.ActorId);
    }
}

public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.HasKey(p => p.UserId);
    }
}

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.EventType).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Channel).HasMaxLength(20).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.DeliveryStatus).HasMaxLength(50).IsRequired();

        builder.HasIndex(n => new { n.UserId, n.SentAtUtc });
    }
}
