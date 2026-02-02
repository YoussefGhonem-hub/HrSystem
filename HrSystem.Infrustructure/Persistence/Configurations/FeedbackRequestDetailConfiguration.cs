using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class FeedbackRequestDetailConfiguration : IEntityTypeConfiguration<FeedbackRequestDetail>
{
    public void Configure(EntityTypeBuilder<FeedbackRequestDetail> builder)
    {
        builder.ToTable("FeedbackRequestDetails", "Requests");

        builder.Property(f => f.FeedbackContent).IsRequired().HasMaxLength(4000);
        builder.Property(f => f.IsAnonymous).HasDefaultValue(false);
        builder.Property(f => f.Rating).HasDefaultValue(0);
        builder.Property(f => f.TargetDepartment).HasMaxLength(200);
        builder.Property(f => f.TargetPerson).HasMaxLength(200);
        builder.Property(f => f.SuggestedImprovement).HasMaxLength(2000);
        builder.Property(f => f.ResponseRequired).HasDefaultValue(false);
        builder.Property(f => f.ResponseContent).HasMaxLength(4000);

        builder.HasOne(f => f.EmployeeRequest)
            .WithOne(r => r.FeedbackDetail)
            .HasForeignKey<FeedbackRequestDetail>(f => f.EmployeeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.FeedbackType)
            .WithMany(ft => ft.FeedbackRequests)
            .HasForeignKey(f => f.FeedbackTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
