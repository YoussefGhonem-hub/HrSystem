using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class SocialInsuranceRateConfiguration : IEntityTypeConfiguration<SocialInsuranceRate>
{
    public void Configure(EntityTypeBuilder<SocialInsuranceRate> builder)
    {
        builder.ToTable("SocialInsuranceRates", "Payroll");

        builder.Property(sir => sir.EmployeeRate).HasColumnType("decimal(5,2)");
        builder.Property(sir => sir.EmployerRate).HasColumnType("decimal(5,2)");
        builder.Property(sir => sir.MinSalaryBase).HasColumnType("decimal(18,2)");
        builder.Property(sir => sir.MaxSalaryBase).HasColumnType("decimal(18,2)");

        builder.HasIndex(sir => sir.Year).IsUnique();
    }
}
