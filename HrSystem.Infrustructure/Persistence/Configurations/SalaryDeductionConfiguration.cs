using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class SalaryDeductionConfiguration : IEntityTypeConfiguration<SalaryDeduction>
{
    public void Configure(EntityTypeBuilder<SalaryDeduction> builder)
    {
        builder.ToTable("SalaryDeductions", "Payroll");

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

        builder.Property(x => x.IsRecurring)
            .HasDefaultValue(true);

        builder.Property(x => x.IsPercentage)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Salary)
            .WithMany(s => s.Deductions)
            .HasForeignKey(x => x.SalaryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
