using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class PayrollCycleConfiguration : IEntityTypeConfiguration<PayrollCycle>
{
    public void Configure(EntityTypeBuilder<PayrollCycle> builder)
    {
        builder.ToTable("PayrollCycles", "Payroll");

        builder.Property(pc => pc.CycleName).IsRequired().HasMaxLength(100);
        builder.Property(pc => pc.TotalGrossSalary).HasColumnType("decimal(18,2)");
        builder.Property(pc => pc.TotalNetSalary).HasColumnType("decimal(18,2)");
        builder.Property(pc => pc.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(pc => pc.TotalTax).HasColumnType("decimal(18,2)");
        builder.Property(pc => pc.TotalInsurance).HasColumnType("decimal(18,2)");

        builder.HasIndex(pc => new { pc.Month, pc.Year }).IsUnique();
    }
}
