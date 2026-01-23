using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans", "Payroll");

        builder.Property(l => l.LoanName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.RemainingAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.MonthlyDeduction).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Notes).HasMaxLength(1000);

        builder.HasIndex(l => l.EmployeeId);
        builder.HasIndex(l => l.IsActive);

        builder.HasOne(l => l.Employee)
            .WithMany()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
