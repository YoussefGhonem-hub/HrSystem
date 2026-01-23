using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OvertimeRequestConfiguration : IEntityTypeConfiguration<OvertimeRequest>
{
    public void Configure(EntityTypeBuilder<OvertimeRequest> builder)
    {
        builder.ToTable("OvertimeRequests", "Attendance");

        builder.Property(o => o.Reason).IsRequired().HasMaxLength(500);
        builder.Property(o => o.Status).HasMaxLength(50);

        builder.HasOne(o => o.Employee)
            .WithMany()
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Status)
            .WithMany(os => os.OvertimeRequests)
            .HasForeignKey(o => o.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
