using HrSystem.Domain.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeLeaveTransactionConfiguration : IEntityTypeConfiguration<EmployeeLeaveTransaction>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveTransaction> builder)
    {
        builder.ToTable("EmployeeLeaveTransactions", "Leave");

        builder.Property(t => t.DaysChanged).HasPrecision(9, 2);
        builder.Property(t => t.BalanceAfter).HasPrecision(9, 2);
        builder.Property(t => t.ReferenceType).HasMaxLength(100);
        builder.Property(t => t.Notes).HasMaxLength(1000);

        builder.HasIndex(t => new { t.EmployeeId, t.VacationTypeId, t.Year });
        builder.HasIndex(t => t.EmployeeLeaveBalanceId);

        builder.HasOne(t => t.LeaveBalance)
            .WithMany(b => b.Transactions)
            .HasForeignKey(t => t.EmployeeLeaveBalanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
