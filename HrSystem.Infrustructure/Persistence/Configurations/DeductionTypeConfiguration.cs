using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class DeductionTypeConfiguration : IEntityTypeConfiguration<DeductionType>
{
    public void Configure(EntityTypeBuilder<DeductionType> builder)
    {
        builder.ToTable("DeductionTypes", "Payroll");

        builder.Property(d => d.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(d => d.NameEn).IsRequired().HasMaxLength(200);
    }
}
