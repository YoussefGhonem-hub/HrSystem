using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeRequestConfiguration : IEntityTypeConfiguration<EmployeeRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeRequest> builder)
    {
        builder.ToTable("EmployeeRequests", "Requests");

        builder.Property(r => r.Title).IsRequired().HasMaxLength(250);
        builder.Property(r => r.Description).HasMaxLength(2000);
        builder.Property(r => r.AttachmentUrl).HasMaxLength(1024);
        builder.Property(r => r.ManagerComments).HasMaxLength(1000);
        builder.Property(r => r.RejectionReason).HasMaxLength(1000);

        builder.HasIndex(r => new { r.EmployeeId, r.RequestType });
        builder.HasIndex(r => new { r.BranchId, r.RequestType });
        builder.HasIndex(r => new { r.TenantId, r.Status });

        builder.HasOne(r => r.Employee)
            .WithMany(e => e.EmployeeRequests)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ApprovedByUser)
            .WithMany()
            .HasForeignKey(r => r.ApprovedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.ProcessedByUser)
            .WithMany()
            .HasForeignKey(r => r.ProcessedBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
