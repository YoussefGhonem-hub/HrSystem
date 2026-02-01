using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class MiscellaneousRequestDetailConfiguration : IEntityTypeConfiguration<MiscellaneousRequestDetail>
{
    public void Configure(EntityTypeBuilder<MiscellaneousRequestDetail> builder)
    {
        builder.ToTable("MiscellaneousRequestDetails", "Requests");

        builder.Property(m => m.AdditionalNotes).HasMaxLength(2000);
        builder.Property(m => m.ReferenceNumber).HasMaxLength(100);
        builder.Property(m => m.Priority).HasMaxLength(50);
        builder.Property(m => m.ExpectedCompletionDate);

        builder.HasOne(m => m.EmployeeRequest)
            .WithOne(r => r.MiscellaneousDetail)
            .HasForeignKey<MiscellaneousRequestDetail>(m => m.EmployeeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.MiscellaneousType)
            .WithMany(mt => mt.MiscellaneousRequests)
            .HasForeignKey(m => m.MiscellaneousTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
