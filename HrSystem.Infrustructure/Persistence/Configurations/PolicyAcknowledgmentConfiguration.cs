using HrSystem.Domain.Entities.Lifecycle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PolicyAcknowledgmentConfiguration : IEntityTypeConfiguration<PolicyAcknowledgment>
{
    public void Configure(EntityTypeBuilder<PolicyAcknowledgment> builder)
    {
        builder.ToTable("PolicyAcknowledgments", "Lifecycle");

        builder.Property(pa => pa.PolicyName).IsRequired().HasMaxLength(200);
        builder.Property(pa => pa.PolicyVersion).IsRequired().HasMaxLength(50);

        builder.HasOne(pa => pa.Employee)
            .WithMany()
            .HasForeignKey(pa => pa.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pa => new { pa.EmployeeId, pa.PolicyName, pa.PolicyVersion });
    }
}
