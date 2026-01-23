using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeBiometricConfiguration : IEntityTypeConfiguration<EmployeeBiometric>
{
    public void Configure(EntityTypeBuilder<EmployeeBiometric> builder)
    {
        builder.ToTable("EmployeeBiometrics", "Attendance");

        builder.Property(b => b.BiometricType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.TemplateHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.TemplateData)
            .HasMaxLength(4000);

        builder.Property(b => b.Provider)
            .HasMaxLength(100);

        builder.Property(b => b.DeviceId)
            .HasMaxLength(200);

        builder.HasIndex(b => new { b.EmployeeId, b.BiometricType }).IsUnique();

        builder.HasOne(b => b.Employee)
            .WithMany(e => e.Biometrics)
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
