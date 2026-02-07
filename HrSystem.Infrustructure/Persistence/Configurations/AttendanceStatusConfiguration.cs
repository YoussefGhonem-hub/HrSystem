using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class AttendanceStatusConfiguration : IEntityTypeConfiguration<AttendanceStatus>
{
    public void Configure(EntityTypeBuilder<AttendanceStatus> builder)
    {
        builder.ToTable("AttendanceStatuses", "Attendance");

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ColorCode)
            .HasMaxLength(20);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(1);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.TenantId, x.NameEn })
            .IsUnique();

        builder.HasMany(x => x.Attendances)
            .WithOne(a => a.Status)
            .HasForeignKey(a => a.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
