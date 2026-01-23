using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class InvoiceStatusConfiguration : IEntityTypeConfiguration<InvoiceStatus>
{
    public void Configure(EntityTypeBuilder<InvoiceStatus> builder)
    {
        builder.ToTable("InvoiceStatuses", "Organization");

        builder.HasIndex(ist => ist.Code).IsUnique();

        builder.Property(ist => ist.Code).IsRequired().HasMaxLength(50);
        builder.Property(ist => ist.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(ist => ist.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(ist => ist.ColorCode).HasMaxLength(20);
    }
}
