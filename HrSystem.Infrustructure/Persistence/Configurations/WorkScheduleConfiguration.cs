using HrSystem.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class WorkScheduleConfiguration : IEntityTypeConfiguration<WorkSchedule>
{
    public void Configure(EntityTypeBuilder<WorkSchedule> builder)
    {
        builder.ToTable("WorkSchedules", "Attendance");

        builder.Property(ws => ws.Name).IsRequired().HasMaxLength(200);
    }
}
