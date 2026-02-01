using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class BranchRequestSettingConfiguration : IEntityTypeConfiguration<BranchRequestSetting>
{
    public void Configure(EntityTypeBuilder<BranchRequestSetting> builder)
    {
        builder.ToTable("BranchRequestSettings", "Requests");

        builder.Property(s => s.BranchId).IsRequired();
        builder.Property(s => s.CustomInstructions).HasMaxLength(1000);

        builder.HasIndex(s => new { s.BranchId, s.RequestType }).IsUnique();
        builder.HasIndex(s => new { s.TenantId, s.RequestType });

        builder.HasOne(s => s.Branch)
            .WithMany(b => b.RequestSettings)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
