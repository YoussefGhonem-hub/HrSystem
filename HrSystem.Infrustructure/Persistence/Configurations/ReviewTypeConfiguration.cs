using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class ReviewTypeConfiguration : IEntityTypeConfiguration<ReviewType>
{
    public void Configure(EntityTypeBuilder<ReviewType> builder)
    {
        builder.ToTable("ReviewTypes", "Performance");

        builder.HasIndex(rt => rt.Code).IsUnique();

        builder.Property(rt => rt.Code).IsRequired().HasMaxLength(50);
        builder.Property(rt => rt.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(rt => rt.NameEn).IsRequired().HasMaxLength(200);
    }
}
