using HrSystem.Domain.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class LeavePolicyConfiguration : IEntityTypeConfiguration<LeavePolicy>
{
    public void Configure(EntityTypeBuilder<LeavePolicy> builder)
    {
        builder.ToTable("LeavePolicies", "Leave");

        builder.Property(lp => lp.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(lp => lp.NameEn).IsRequired().HasMaxLength(200);

        builder.HasIndex(lp => lp.LeaveTypeId).IsUnique();

        builder.HasOne(lp => lp.LeaveType)
            .WithMany()
            .HasForeignKey(lp => lp.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
