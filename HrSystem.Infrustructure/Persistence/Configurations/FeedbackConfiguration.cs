using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks", "Performance");

        builder.Property(f => f.FeedbackType).HasMaxLength(100);
        builder.Property(f => f.Rating).HasColumnType("decimal(18,2)");

        builder.HasOne(f => f.PerformanceReview)
            .WithMany(pr => pr.Feedbacks)
            .HasForeignKey(f => f.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Provider)
            .WithMany()
            .HasForeignKey(f => f.ProvidedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
