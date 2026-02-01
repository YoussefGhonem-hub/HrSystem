using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class VacationRequestDetailConfiguration : IEntityTypeConfiguration<VacationRequestDetail>
{
    public void Configure(EntityTypeBuilder<VacationRequestDetail> builder)
    {
        builder.ToTable("VacationRequestDetails", "Requests");

        builder.Property(v => v.TotalDays).HasPrecision(5, 2);
        builder.Property(v => v.ManagerComments).HasMaxLength(1000);
        builder.Property(v => v.RejectionReason).HasMaxLength(1000);
        builder.Property(v => v.HRComments).HasMaxLength(1000);
        builder.Property(v => v.EmergencyContactName).HasMaxLength(200);
        builder.Property(v => v.EmergencyContactPhone).HasMaxLength(50);

        builder.HasOne(v => v.EmployeeRequest)
            .WithOne(r => r.VacationDetail)
            .HasForeignKey<VacationRequestDetail>(v => v.EmployeeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.VacationType)
            .WithMany(t => t.VacationRequests)
            .HasForeignKey(v => v.VacationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Manager)
            .WithMany()
            .HasForeignKey(v => v.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // LeaveType relationship - maps VacationType to LeaveType for balance tracking
        builder.HasOne(v => v.LeaveType)
            .WithMany()
            .HasForeignKey(v => v.LeaveTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        // LeavePolicy relationship - for balance deduction rules
        builder.HasOne(v => v.LeavePolicy)
            .WithMany()
            .HasForeignKey(v => v.LeavePolicyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
