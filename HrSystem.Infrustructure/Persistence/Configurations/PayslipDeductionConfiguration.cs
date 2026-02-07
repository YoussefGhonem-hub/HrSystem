using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PayslipDeductionConfiguration : IEntityTypeConfiguration<PayslipDeduction>
{
    public void Configure(EntityTypeBuilder<PayslipDeduction> builder)
    {
        builder.ToTable("PayslipDeductions", "Payroll");

        builder.Property(x => x.DeductionNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DeductionNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Payslip)
            .WithMany(p => p.PayslipDeductions)
            .HasForeignKey(x => x.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
