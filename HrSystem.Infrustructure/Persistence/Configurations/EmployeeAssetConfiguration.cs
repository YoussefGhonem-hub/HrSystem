using HrSystem.Domain.Entities.Lifecycle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeAssetConfiguration : IEntityTypeConfiguration<EmployeeAsset>
{
    public void Configure(EntityTypeBuilder<EmployeeAsset> builder)
    {
        builder.ToTable("EmployeeAssets", "Lifecycle");

        builder.Property(ea => ea.AssetType).IsRequired().HasMaxLength(100);
        builder.Property(ea => ea.AssetName).IsRequired().HasMaxLength(200);
        builder.Property(ea => ea.Condition).HasMaxLength(50);
        builder.Property(ea => ea.Value).HasColumnType("decimal(18,2)");

        builder.HasOne(ea => ea.Employee)
            .WithMany(e => e.Assets)
            .HasForeignKey(ea => ea.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
