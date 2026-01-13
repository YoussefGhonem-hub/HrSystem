using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class TaxBracketConfiguration : IEntityTypeConfiguration<TaxBracket>
{
    public void Configure(EntityTypeBuilder<TaxBracket> builder)
    {
        builder.ToTable("TaxBrackets", "Payroll");

        builder.Property(tb => tb.MinIncome).HasColumnType("decimal(18,2)");
        builder.Property(tb => tb.MaxIncome).HasColumnType("decimal(18,2)");
        builder.Property(tb => tb.TaxRate).HasColumnType("decimal(5,2)");
        builder.Property(tb => tb.FixedAmount).HasColumnType("decimal(18,2)");
    }
}
