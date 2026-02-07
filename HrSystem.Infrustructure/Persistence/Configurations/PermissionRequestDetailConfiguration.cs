using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PermissionRequestDetailConfiguration : IEntityTypeConfiguration<PermissionRequestDetail>
{
    public void Configure(EntityTypeBuilder<PermissionRequestDetail> builder)
    {
        builder.ToTable("PermissionRequestDetails", "Requests");

        builder.Property(p => p.TotalHours).HasPrecision(5, 2);
        builder.Property(p => p.LeaveDeduction).HasPrecision(5, 2);
        builder.Property(p => p.Reason).IsRequired().HasMaxLength(500);
        builder.Property(p => p.ManagerComments).HasMaxLength(1000);
        builder.Property(p => p.RejectionReason).HasMaxLength(1000);

        builder.HasOne(p => p.EmployeeRequest)
            .WithOne(r => r.PermissionDetail)
            .HasForeignKey<PermissionRequestDetail>(p => p.EmployeeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.PermissionType)
            .WithMany(t => t.PermissionRequests)
            .HasForeignKey(p => p.PermissionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Manager)
            .WithMany()
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
