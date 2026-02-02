using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PersonalRequestDetailConfiguration : IEntityTypeConfiguration<PersonalRequestDetail>
{
    public void Configure(EntityTypeBuilder<PersonalRequestDetail> builder)
    {
        builder.ToTable("PersonalRequestDetails", "Requests");

        builder.Property(p => p.Reason).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.IsUrgent).HasDefaultValue(false);
        builder.Property(p => p.RequiresConfidentiality).HasDefaultValue(false);
        builder.Property(p => p.PreferredContactMethod).HasMaxLength(100);
        builder.Property(p => p.AdditionalContactInfo).HasMaxLength(500);

        builder.HasOne(p => p.EmployeeRequest)
            .WithOne(r => r.PersonalDetail)
            .HasForeignKey<PersonalRequestDetail>(p => p.EmployeeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.PersonalType)
            .WithMany(pt => pt.PersonalRequests)
            .HasForeignKey(p => p.PersonalTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
