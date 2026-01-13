using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", "Organization");

        builder.HasIndex(o => o.Code).IsUnique();
        builder.HasIndex(o => o.CommercialRegistrationNumber).IsUnique();
        builder.HasIndex(o => o.TaxRegistrationNumber);

        builder.Property(o => o.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(o => o.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Code).IsRequired().HasMaxLength(50);
        builder.Property(o => o.Email).HasMaxLength(200);
        builder.Property(o => o.PhoneNumber).HasMaxLength(20);
        builder.Property(o => o.CurrentStorageGB).HasColumnType("decimal(18,2)");

        builder.HasOne(o => o.SubscriptionPlan)
            .WithMany(sp => sp.Organizations)
            .HasForeignKey(o => o.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
