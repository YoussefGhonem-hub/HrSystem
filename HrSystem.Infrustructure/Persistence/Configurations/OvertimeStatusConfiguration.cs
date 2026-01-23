using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OvertimeStatusConfiguration : IEntityTypeConfiguration<OvertimeStatus>
{
    public void Configure(EntityTypeBuilder<OvertimeStatus> builder)
    {
        builder.ToTable("OvertimeStatuses", "Attendance");

        builder.HasIndex(os => os.Code).IsUnique();

        builder.Property(os => os.Code).IsRequired().HasMaxLength(50);
        builder.Property(os => os.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(os => os.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(os => os.ColorCode).HasMaxLength(20);
    }
}
