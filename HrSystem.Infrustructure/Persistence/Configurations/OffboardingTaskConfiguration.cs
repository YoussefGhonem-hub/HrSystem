using HrSystem.Domain.Entities.Lifecycle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OffboardingTaskConfiguration : IEntityTypeConfiguration<OffboardingTask>
{
    public void Configure(EntityTypeBuilder<OffboardingTask> builder)
    {
        builder.ToTable("OffboardingTasks", "Lifecycle");

        builder.Property(ot => ot.TaskNameAr).IsRequired().HasMaxLength(200);
        builder.Property(ot => ot.TaskNameEn).IsRequired().HasMaxLength(200);
        builder.Property(ot => ot.Category).HasMaxLength(100);

        builder.HasOne(ot => ot.Employee)
            .WithMany()
            .HasForeignKey(ot => ot.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
