using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasIndex(sp => sp.Code).IsUnique();

        builder.Property(sp => sp.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(sp => sp.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(sp => sp.Code).IsRequired().HasMaxLength(50);
        builder.Property(sp => sp.MonthlyPrice).HasColumnType("decimal(18,2)");
        builder.Property(sp => sp.AnnualPrice).HasColumnType("decimal(18,2)");
        builder.Property(sp => sp.Currency).HasMaxLength(10);
    }
}
