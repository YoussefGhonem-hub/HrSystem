using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class SalaryAllowanceConfiguration : IEntityTypeConfiguration<SalaryAllowance>
{
    public void Configure(EntityTypeBuilder<SalaryAllowance> builder)
    {
        builder.ToTable("SalaryAllowances", "Payroll");

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PercentageValue)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.IsTaxable)
            .HasDefaultValue(true);

        builder.Property(x => x.IsSubjectToInsurance)
            .HasDefaultValue(true);

        builder.Property(x => x.IsPercentage)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Salary)
            .WithMany(s => s.Allowances)
            .HasForeignKey(x => x.SalaryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
