using HrSystem.Domain.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests", "Leave");

        builder.Property(l => l.TotalDays).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Reason).IsRequired().HasMaxLength(500);

        builder.HasOne(l => l.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.LeavePolicy)
            .WithMany()
            .HasForeignKey(l => l.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Manager)
            .WithMany()
            .HasForeignKey(l => l.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
