using MedInsight.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedInsight.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.FullName);
    }
}

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.DateOfBirth).HasColumnType("date");

        // USERS 1:1 PATIENTS (bkz. docs/domain/erd-identity-case.md)
        builder.HasOne<User>().WithOne().HasForeignKey<Patient>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).HasMaxLength(100);
        builder.Property(d => d.Specialty).HasMaxLength(200).IsRequired();
        builder.Property(d => d.LicenseNumber).HasMaxLength(100).IsRequired();
        builder.Ignore(d => d.DomainEvents);
        builder.Ignore(d => d.ComputedStatus);
        builder.Ignore(d => d.EffectiveStatus);

        builder.HasOne<User>().WithOne().HasForeignKey<Doctor>(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(d => d.UserId).IsUnique();
        builder.HasIndex(d => d.Specialty);

        builder.HasMany(d => d.Verifications).WithOne().HasForeignKey(v => v.DoctorId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class DoctorVerificationConfiguration : IEntityTypeConfiguration<DoctorVerification>
{
    public void Configure(EntityTypeBuilder<DoctorVerification> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.DocumentUrl).HasMaxLength(1000).IsRequired();
        builder.Property(v => v.QrPayload).HasMaxLength(4000);
        builder.Property(v => v.QrParsedData).HasColumnType("jsonb");
        builder.Property(v => v.RejectionReason).HasMaxLength(1000);
        builder.HasIndex(v => v.DoctorId);
    }
}

public sealed class ReviewerProfileConfiguration : IEntityTypeConfiguration<ReviewerProfile>
{
    public void Configure(EntityTypeBuilder<ReviewerProfile> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Specialty).HasMaxLength(200).IsRequired();
        builder.Property(p => p.CorrectionRate).HasColumnType("numeric(5,4)");
        builder.Property(p => p.AverageResponseTimeMinutes).HasColumnType("numeric(10,2)");
        builder.Property("_correctionCount").HasColumnName("correction_count");

        builder.HasOne<Doctor>().WithOne().HasForeignKey<ReviewerProfile>(p => p.DoctorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.DoctorId).IsUnique();
    }
}

public sealed class CaregiverConfiguration : IEntityTypeConfiguration<Caregiver>
{
    public void Configure(EntityTypeBuilder<Caregiver> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.RelationshipType).HasMaxLength(100).IsRequired();

        builder.HasOne<User>().WithOne().HasForeignKey<Caregiver>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => c.UserId).IsUnique();
    }
}
