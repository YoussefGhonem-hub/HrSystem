using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class TrainingRequestDetailConfiguration : IEntityTypeConfiguration<TrainingRequestDetail>
{
    public void Configure(EntityTypeBuilder<TrainingRequestDetail> builder)
    {
        builder.ToTable("TrainingRequestDetails", "Requests");

        builder.Property(t => t.TrainingName).IsRequired().HasMaxLength(300);
        builder.Property(t => t.TrainingProvider).HasMaxLength(200);
        builder.Property(t => t.TrainingLocation).HasMaxLength(300);
        builder.Property(t => t.EstimatedCost).HasPrecision(18, 2);
        builder.Property(t => t.ApprovedBudget).HasPrecision(18, 2);
        builder.Property(t => t.Currency).HasMaxLength(10).HasDefaultValue("EGP");
        builder.Property(t => t.Objectives).HasMaxLength(2000);
        builder.Property(t => t.ExpectedOutcome).HasMaxLength(2000);
        builder.Property(t => t.CertificateUrl).HasMaxLength(1024);

        builder.HasOne(t => t.EmployeeRequest)
            .WithOne(r => r.TrainingDetail)
            .HasForeignKey<TrainingRequestDetail>(t => t.EmployeeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.TrainingType)
            .WithMany(tt => tt.TrainingRequests)
            .HasForeignKey(t => t.TrainingTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
