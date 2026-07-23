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

        builder.HasOne<User>().WithOne().HasForeignKey<Doctor>(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(d => d.UserId).IsUnique();
        builder.HasIndex(d => d.Specialty);
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
