using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

/// <summary>
/// One payroll-cycle configuration row per organization. The organization link is the
/// tenant id itself (no separate shadow OrganizationId column), so upserts that only set
/// TenantId never violate the foreign key.
/// </summary>
public class OrganizationPayrollSettingsConfiguration : IEntityTypeConfiguration<OrganizationPayrollSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationPayrollSettings> builder)
    {
        builder.ToTable("OrganizationPayrollSettings");

        builder.Property(s => s.CycleType).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(1000);

        builder.HasOne(s => s.Organization)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Only one live settings row per organization (soft-deleted rows don't block re-creation).
        builder.HasIndex(s => s.TenantId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
