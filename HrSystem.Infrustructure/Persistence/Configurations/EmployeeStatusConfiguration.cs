using HrSystem.Domain.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeStatusConfiguration : IEntityTypeConfiguration<EmployeeStatus>
{
    public void Configure(EntityTypeBuilder<EmployeeStatus> builder)
    {
        builder.ToTable("EmployeeStatuses", "Employee");

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ColorCode)
            .HasMaxLength(20);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(1);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.TenantId, x.NameEn })
            .IsUnique();

        builder.HasMany(x => x.Employees)
            .WithOne(e => e.Status)
            .HasForeignKey(e => e.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
