using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OrganizationSettingsConfiguration : IEntityTypeConfiguration<OrganizationSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationSettings> builder)
    {
        builder.ToTable("OrganizationSettings", "Organization");

        builder.HasIndex(os => new { os.OrganizationId, os.SettingKey }).IsUnique();

        builder.Property(os => os.SettingKey).IsRequired().HasMaxLength(100);
        builder.Property(os => os.SettingValue).IsRequired();

        builder.HasOne(os => os.Organization)
            .WithMany(o => o.Settings)
            .HasForeignKey(os => os.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
