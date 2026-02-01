using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PermissionTypeConfiguration : IEntityTypeConfiguration<PermissionType>
{
    public void Configure(EntityTypeBuilder<PermissionType> builder)
    {
        builder.ToTable("PermissionTypes", "Requests");

        builder.Property(p => p.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(p => p.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.MaxHours).HasPrecision(5, 2);
        builder.Property(p => p.HoursPerLeaveDay).HasPrecision(5, 2);

        builder.HasIndex(p => new { p.TenantId, p.NameEn }).IsUnique();
    }
}
