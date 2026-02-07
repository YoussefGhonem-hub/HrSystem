using HrSystem.Domain.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class MaritalStatusConfiguration : IEntityTypeConfiguration<MaritalStatus>
{
    public void Configure(EntityTypeBuilder<MaritalStatus> builder)
    {
        builder.ToTable("MaritalStatuses", "Employee");

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(1);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.TenantId, x.NameEn })
            .IsUnique();

        builder.HasMany(x => x.Employees)
            .WithOne(e => e.MaritalStatus)
            .HasForeignKey(e => e.MaritalStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
