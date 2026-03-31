using HrSystem.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class BankExportProfileConfiguration : IEntityTypeConfiguration<BankExportProfile>
{
    public void Configure(EntityTypeBuilder<BankExportProfile> builder)
    {
        builder.ToTable("BankExportProfiles", "Payroll");

        builder.Property(b => b.ProfileName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.BankName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.CompanyAccountNumber).IsRequired().HasMaxLength(100);
        builder.Property(b => b.CompanyAccountName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(10);
        builder.Property(b => b.BicCode).HasMaxLength(50);
        builder.Property(b => b.Narrative).HasMaxLength(200);
        builder.Property(b => b.TemplateFileKey).HasMaxLength(500);
        builder.Property(b => b.TemplateFileName).HasMaxLength(300);
    }
}
