using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PublicHolidayConfiguration : IEntityTypeConfiguration<PublicHoliday>
{
    public void Configure(EntityTypeBuilder<PublicHoliday> builder)
    {
        builder.ToTable("PublicHolidays", "Attendance");

        builder.Property(ph => ph.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(ph => ph.NameEn).IsRequired().HasMaxLength(200);

        builder.HasIndex(ph => new { ph.Date, ph.Year });
    }
}
