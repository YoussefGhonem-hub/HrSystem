using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class BranchWorkScheduleConfiguration : IEntityTypeConfiguration<BranchWorkSchedule>
{
    public void Configure(EntityTypeBuilder<BranchWorkSchedule> builder)
    {
        builder.ToTable("BranchWorkSchedules", "Organization");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TimeZone)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ShiftTotalHours)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.MinimumFullDayHours)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.MinimumHalfDayHours)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.AbsentThresholdHours)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.OvertimeStartsAfterHours)
            .HasColumnType("decimal(5,2)");

        builder.HasOne(x => x.Branch)
            .WithMany(b => b.WorkSchedules)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.BranchId);
    }
}
