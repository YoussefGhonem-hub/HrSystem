using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class BranchAttendanceSettingConfiguration : IEntityTypeConfiguration<BranchAttendanceSetting>
{
    public void Configure(EntityTypeBuilder<BranchAttendanceSetting> builder)
    {
        builder.ToTable("BranchAttendanceSettings", "Attendance");

        builder.HasKey(x => x.Id);

        // One setting per branch
        builder.HasIndex(x => x.BranchId).IsUnique();

        // ── Enum stored as string ────────────────────────────
        builder.Property(x => x.PrimaryMethod)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // ── Location settings ────────────────────────────────
        builder.Property(x => x.DefaultGeofenceRadiusMeters)
            .HasDefaultValue(200);

        // ── FaceId settings ──────────────────────────────────
        builder.Property(x => x.FaceIdConfidenceThreshold)
            .HasDefaultValue(0.85);

        // ── Excel settings ───────────────────────────────────
        builder.Property(x => x.ExcelDateFormat)
            .HasMaxLength(50);

        // ── Notes ────────────────────────────────────────────
        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // ── Relationships ────────────────────────────────────
        builder.HasOne(x => x.Branch)
            .WithOne(b => b.AttendanceSetting)
            .HasForeignKey<BranchAttendanceSetting>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CheckInPoints)
            .WithOne(p => p.BranchAttendanceSetting)
            .HasForeignKey(p => p.BranchAttendanceSettingId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
