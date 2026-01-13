using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class SalaryConfiguration : IEntityTypeConfiguration<Salary>
{
    public void Configure(EntityTypeBuilder<Salary> builder)
    {
        builder.ToTable("Salaries", "Payroll");

        builder.Property(s => s.BasicSalary).HasColumnType("decimal(18,2)");

        builder.HasOne(s => s.Employee)
            .WithMany(e => e.Salaries)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
