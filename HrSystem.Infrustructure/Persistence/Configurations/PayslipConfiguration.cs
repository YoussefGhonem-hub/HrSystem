using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips", "Payroll");

        builder.HasIndex(p => p.PayslipNumber).IsUnique();

        builder.Property(p => p.PayslipNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalAllowances).HasColumnType("decimal(18,2)");
        builder.Property(p => p.GrossSalary).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(p => p.IncomeTax).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.PayrollCycle)
            .WithMany(pc => pc.Payslips)
            .HasForeignKey(p => p.PayrollCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
