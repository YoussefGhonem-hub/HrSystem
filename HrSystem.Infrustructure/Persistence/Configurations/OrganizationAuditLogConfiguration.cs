using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OrganizationAuditLogConfiguration : IEntityTypeConfiguration<OrganizationAuditLog>
{
    public void Configure(EntityTypeBuilder<OrganizationAuditLog> builder)
    {
        builder.ToTable("OrganizationAuditLogs", "Organization");

        builder.Property(oal => oal.Action).IsRequired().HasMaxLength(100);
        builder.Property(oal => oal.EntityName).IsRequired().HasMaxLength(200);

        builder.HasOne(oal => oal.Organization)
            .WithMany()
            .HasForeignKey(oal => oal.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(oal => new { oal.OrganizationId, oal.CreatedDate });
        builder.HasIndex(oal => oal.EntityName);
    }
}
