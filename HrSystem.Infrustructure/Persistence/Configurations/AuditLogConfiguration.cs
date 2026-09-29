using HrSystem.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "Audit");

        builder.Property(a => a.Module).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityDisplay).HasMaxLength(300);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(20);
        builder.Property(a => a.ChangesJson).HasColumnType("nvarchar(max)");
        builder.Property(a => a.UserName).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(64);

        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => new { a.Module, a.Timestamp });
        builder.HasIndex(a => a.UserId);
    }
}
