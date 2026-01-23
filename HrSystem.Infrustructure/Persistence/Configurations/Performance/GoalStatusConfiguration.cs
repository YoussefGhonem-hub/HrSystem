using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations.Performance;

public class GoalStatusConfiguration : IEntityTypeConfiguration<GoalStatus>
{
    public void Configure(EntityTypeBuilder<GoalStatus> builder)
    {
        builder.ToTable("GoalStatuses", "Performance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(500);

        builder.Property(x => x.DescriptionEn)
            .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
