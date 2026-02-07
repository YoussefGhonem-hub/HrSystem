using HrSystem.Domain.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeLeaveBalanceConfiguration : IEntityTypeConfiguration<EmployeeLeaveBalance>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveBalance> builder)
    {
        builder.ToTable("EmployeeLeaveBalances", "Leave");

        builder.Property(b => b.Year).IsRequired();
        builder.Property(b => b.AllocatedDays).HasPrecision(9, 2).HasDefaultValue(0m);
        builder.Property(b => b.CarryOverDays).HasPrecision(9, 2).HasDefaultValue(0m);
        builder.Property(b => b.ManualAdjustmentDays).HasPrecision(9, 2).HasDefaultValue(0m);
        builder.Property(b => b.UsedDays).HasPrecision(9, 2).HasDefaultValue(0m);
        builder.Property(b => b.Notes).HasMaxLength(1000);

        builder.HasIndex(b => new { b.EmployeeId, b.VacationTypeId, b.Year }).IsUnique();

        builder.HasOne(b => b.Employee)
            .WithMany()
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.VacationType)
            .WithMany()
            .HasForeignKey(b => b.VacationTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
