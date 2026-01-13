using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OrganizationModuleConfiguration : IEntityTypeConfiguration<OrganizationModule>
{
    public void Configure(EntityTypeBuilder<OrganizationModule> builder)
    {
        builder.ToTable("OrganizationModules");

        builder.HasIndex(om => new { om.OrganizationId, om.ModuleName }).IsUnique();

        builder.Property(om => om.ModuleName).IsRequired().HasMaxLength(100);

        builder.HasOne(om => om.Organization)
            .WithMany(o => o.Modules)
            .HasForeignKey(om => om.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
