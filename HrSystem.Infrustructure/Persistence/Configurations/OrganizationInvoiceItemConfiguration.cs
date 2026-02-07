using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OrganizationInvoiceItemConfiguration : IEntityTypeConfiguration<OrganizationInvoiceItem>
{
    public void Configure(EntityTypeBuilder<OrganizationInvoiceItem> builder)
    {
        builder.ToTable("OrganizationInvoiceItems", "Organization");

        builder.Property(x => x.DescriptionAr)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.DescriptionEn)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Quantity)
            .HasDefaultValue(1);

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
