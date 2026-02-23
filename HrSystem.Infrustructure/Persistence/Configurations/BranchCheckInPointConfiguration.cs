using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class BranchCheckInPointConfiguration : IEntityTypeConfiguration<BranchCheckInPoint>
{
    public void Configure(EntityTypeBuilder<BranchCheckInPoint> builder)
    {
        builder.ToTable("BranchCheckInPoints", "Attendance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Latitude)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .IsRequired();

        builder.HasIndex(x => x.BranchId);
        builder.HasIndex(x => x.BranchAttendanceSettingId);

        // ── Relationships ────────────────────────────────────
        builder.HasOne(x => x.Branch)
            .WithMany(b => b.CheckInPoints)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BranchAttendanceSetting)
            .WithMany(s => s.CheckInPoints)
            .HasForeignKey(x => x.BranchAttendanceSettingId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
