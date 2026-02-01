using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OvertimeRequestDetailConfiguration : IEntityTypeConfiguration<OvertimeRequestDetail>
{
    public void Configure(EntityTypeBuilder<OvertimeRequestDetail> builder)
    {
        builder.ToTable("OvertimeRequestDetails", "Requests");

        builder.Property(o => o.Multiplier).HasPrecision(4, 2).HasDefaultValue(1.5m);
        builder.Property(o => o.ApprovalNotes).HasMaxLength(1000);
        builder.Property(o => o.ProjectCode).HasMaxLength(50);
        builder.Property(o => o.TaskDescription).HasMaxLength(500);

        builder.HasOne(o => o.EmployeeRequest)
            .WithOne(r => r.OvertimeDetail)
            .HasForeignKey<OvertimeRequestDetail>(o => o.EmployeeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.OvertimeType)
            .WithMany(t => t.OvertimeRequests)
            .HasForeignKey(o => o.OvertimeTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
