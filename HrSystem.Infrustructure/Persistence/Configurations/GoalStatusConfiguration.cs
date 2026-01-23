using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class GoalStatusConfiguration : IEntityTypeConfiguration<GoalStatus>
{
    public void Configure(EntityTypeBuilder<GoalStatus> builder)
    {
        builder.ToTable("GoalStatuses", "Performance");

        builder.HasIndex(gs => gs.Code).IsUnique();

        builder.Property(gs => gs.Code).IsRequired().HasMaxLength(50);
        builder.Property(gs => gs.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(gs => gs.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(gs => gs.ColorCode).HasMaxLength(20);
    }
}
