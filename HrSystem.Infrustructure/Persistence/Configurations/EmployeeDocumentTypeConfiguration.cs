using HrSystem.Domain.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeDocumentTypeConfiguration : IEntityTypeConfiguration<EmployeeDocumentType>
{
    public void Configure(EntityTypeBuilder<EmployeeDocumentType> builder)
    {
        builder.ToTable("EmployeeDocumentTypes", "Employee");

        builder.Property(dt => dt.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(dt => dt.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(dt => dt.Description).HasMaxLength(1000);
        builder.Property(dt => dt.CategoryKey)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(dt => dt.CategoryKey);
        builder.HasIndex(dt => dt.DisplayOrder);
        builder.HasIndex(dt => dt.IsActive);
    }
}
