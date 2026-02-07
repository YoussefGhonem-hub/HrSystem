using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations.Performance;

public class KPIEvaluationConfiguration : IEntityTypeConfiguration<KPIEvaluation>
{
    public void Configure(EntityTypeBuilder<KPIEvaluation> builder)
    {
        builder.ToTable("KPIEvaluations", "Performance");

        builder.Property(x => x.Rating)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.WeightedScore)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Comments)
            .HasMaxLength(1000);

        builder.Property(x => x.Evidence)
            .HasMaxLength(1000);

        builder.HasOne(x => x.PerformanceReview)
            .WithMany(r => r.KPIEvaluations)
            .HasForeignKey(x => x.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.KPI)
            .WithMany()
            .HasForeignKey(x => x.KPIId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
