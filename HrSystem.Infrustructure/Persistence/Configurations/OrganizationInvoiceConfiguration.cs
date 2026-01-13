using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class OrganizationInvoiceConfiguration : IEntityTypeConfiguration<OrganizationInvoice>
{
    public void Configure(EntityTypeBuilder<OrganizationInvoice> builder)
    {
        builder.ToTable("OrganizationInvoices");

        builder.HasIndex(oi => oi.InvoiceNumber).IsUnique();

        builder.Property(oi => oi.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(oi => oi.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.RemainingAmount).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.Status).HasMaxLength(50);

        builder.HasOne(oi => oi.Organization)
            .WithMany()
            .HasForeignKey(oi => oi.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
