using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations.Performance;

public class GoalMilestoneConfiguration : IEntityTypeConfiguration<GoalMilestone>
{
    public void Configure(EntityTypeBuilder<GoalMilestone> builder)
    {
        builder.ToTable("GoalMilestones", "Performance");

        builder.Property(x => x.TitleAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TitleEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.IsCompleted)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Goal)
            .WithMany(g => g.Milestones)
            .HasForeignKey(x => x.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
