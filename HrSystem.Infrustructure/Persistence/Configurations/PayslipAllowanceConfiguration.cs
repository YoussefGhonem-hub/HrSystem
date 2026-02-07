using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PayslipAllowanceConfiguration : IEntityTypeConfiguration<PayslipAllowance>
{
    public void Configure(EntityTypeBuilder<PayslipAllowance> builder)
    {
        builder.ToTable("PayslipAllowances", "Payroll");

        builder.Property(x => x.AllowanceNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AllowanceNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Payslip)
            .WithMany(p => p.PayslipAllowances)
            .HasForeignKey(x => x.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
