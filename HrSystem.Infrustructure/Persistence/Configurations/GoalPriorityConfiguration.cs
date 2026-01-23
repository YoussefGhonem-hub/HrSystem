using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class GoalPriorityConfiguration : IEntityTypeConfiguration<GoalPriority>
{
    public void Configure(EntityTypeBuilder<GoalPriority> builder)
    {
        builder.ToTable("GoalPriorities", "Performance");

        builder.HasIndex(gp => gp.Code).IsUnique();

        builder.Property(gp => gp.Code).IsRequired().HasMaxLength(50);
        builder.Property(gp => gp.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(gp => gp.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(gp => gp.ColorCode).HasMaxLength(20);
    }
}
