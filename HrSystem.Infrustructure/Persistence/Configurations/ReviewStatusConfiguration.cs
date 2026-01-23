using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class ReviewStatusConfiguration : IEntityTypeConfiguration<ReviewStatus>
{
    public void Configure(EntityTypeBuilder<ReviewStatus> builder)
    {
        builder.ToTable("ReviewStatuses", "Performance");

        builder.HasIndex(rs => rs.Code).IsUnique();

        builder.Property(rs => rs.Code).IsRequired().HasMaxLength(50);
        builder.Property(rs => rs.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(rs => rs.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(rs => rs.ColorCode).HasMaxLength(20);
    }
}
