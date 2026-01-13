using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class AllowanceTypeConfiguration : IEntityTypeConfiguration<AllowanceType>
{
    public void Configure(EntityTypeBuilder<AllowanceType> builder)
    {
        builder.ToTable("AllowanceTypes", "Payroll");

        builder.Property(a => a.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(a => a.NameEn).IsRequired().HasMaxLength(200);
    }
}
