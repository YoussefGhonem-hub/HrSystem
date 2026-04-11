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

        builder.Property(x => x.LoanId)
            .IsRequired(false);

        builder.HasOne(x => x.Payslip)
            .WithMany(p => p.PayslipDeductions)
            .HasForeignKey(x => x.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional: link to the Loan that generated this deduction line.
        // SetNull on delete so payslip history is not lost if a loan is removed.
        builder.HasOne(x => x.Loan)
            .WithMany()
            .HasForeignKey(x => x.LoanId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
