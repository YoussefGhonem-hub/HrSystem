using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("Goals", "Performance");

        builder.Property(g => g.TitleAr).IsRequired().HasMaxLength(200);
        builder.Property(g => g.TitleEn).IsRequired().HasMaxLength(200);
        builder.Property(g => g.Status).HasMaxLength(50);
        builder.Property(g => g.Priority).HasMaxLength(50);

        builder.HasOne(g => g.Employee)
            .WithMany()
            .HasForeignKey(g => g.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
