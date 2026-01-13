using HrSystem.Domain.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances", "Leave");

        builder.Property(lb => lb.TotalDays).HasColumnType("decimal(18,2)");
        builder.Property(lb => lb.UsedDays).HasColumnType("decimal(18,2)");
        builder.Property(lb => lb.RemainingDays).HasColumnType("decimal(18,2)");
        builder.Property(lb => lb.CarriedForwardDays).HasColumnType("decimal(18,2)");

        builder.HasOne(lb => lb.Employee)
            .WithMany(e => e.LeaveBalances)
            .HasForeignKey(lb => lb.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lb => lb.LeavePolicy)
            .WithMany(lp => lp.LeaveBalances)
            .HasForeignKey(lb => lb.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(lb => new { lb.EmployeeId, lb.LeavePolicyId, lb.Year }).IsUnique();
    }
}
