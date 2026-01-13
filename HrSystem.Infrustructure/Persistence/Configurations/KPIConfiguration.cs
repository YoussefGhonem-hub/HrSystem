using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class KPIConfiguration : IEntityTypeConfiguration<KPI>
{
    public void Configure(EntityTypeBuilder<KPI> builder)
    {
        builder.ToTable("KPIs", "Performance");

        builder.Property(k => k.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(k => k.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(k => k.Category).HasMaxLength(100);

        builder.HasOne(k => k.JobTitle)
            .WithMany()
            .HasForeignKey(k => k.JobTitleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.Department)
            .WithMany()
            .HasForeignKey(k => k.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
