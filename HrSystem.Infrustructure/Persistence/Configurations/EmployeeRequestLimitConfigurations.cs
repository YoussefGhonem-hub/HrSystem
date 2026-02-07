using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeVacationLimitConfiguration : IEntityTypeConfiguration<EmployeeVacationLimit>
{
    public void Configure(EntityTypeBuilder<EmployeeVacationLimit> builder)
    {
        builder.ToTable("EmployeeVacationLimits", "Requests");

        builder.Property(l => l.MaxDaysPerYear);
        builder.Property(l => l.Notes).HasMaxLength(500);

        builder.HasIndex(l => new { l.EmployeeId, l.VacationTypeId }).IsUnique();

        builder.HasOne(l => l.Employee)
            .WithMany()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.VacationType)
            .WithMany()
            .HasForeignKey(l => l.VacationTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeePermissionLimitConfiguration : IEntityTypeConfiguration<EmployeePermissionLimit>
{
    public void Configure(EntityTypeBuilder<EmployeePermissionLimit> builder)
    {
        builder.ToTable("EmployeePermissionLimits", "Requests");

        builder.Property(l => l.MaxHoursPerMonth).HasPrecision(5, 2);
        builder.Property(l => l.Notes).HasMaxLength(500);

        builder.HasIndex(l => new { l.EmployeeId, l.PermissionTypeId }).IsUnique();

        builder.HasOne(l => l.Employee)
            .WithMany()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.PermissionType)
            .WithMany()
            .HasForeignKey(l => l.PermissionTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
