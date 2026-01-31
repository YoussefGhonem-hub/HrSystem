using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances", "Attendance");

        builder.HasIndex(a => new { a.EmployeeId, a.Date, a.IsConfigurationRecord }).IsUnique();

        builder.Property(a => a.IsConfigurationRecord)
            .HasDefaultValue(false);

        builder.Property(a => a.WorkShift).HasMaxLength(128);
        builder.Property(a => a.WorkDays).HasMaxLength(128);
        builder.Property(a => a.GracePeriod).HasMaxLength(64);
        builder.Property(a => a.MaxLatePerMonth).HasMaxLength(64);
        builder.Property(a => a.OvertimeEligible).HasDefaultValue(false);
        builder.Property(a => a.OvertimeCalculation).HasMaxLength(128);
        builder.Property(a => a.AttendanceMethod).HasMaxLength(128);
        builder.Property(a => a.LateDeductionPolicy).HasMaxLength(128);
        builder.Property(a => a.AbsenceDeductionPolicy).HasMaxLength(128);
        builder.Property(a => a.HalfDayRule).HasMaxLength(128);
        builder.Property(a => a.MissingCheckoutHandling).HasMaxLength(128);

        builder.HasQueryFilter(a => !a.IsConfigurationRecord);

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
