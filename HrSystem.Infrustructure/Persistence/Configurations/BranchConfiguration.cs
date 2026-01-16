using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches", "Organization");

        // Indexes
        builder.HasIndex(b => b.Code).IsUnique();
        builder.HasIndex(b => new { b.OrganizationId, b.IsActive });
        builder.HasIndex(b => b.Country);

        // Properties
        builder.Property(b => b.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(b => b.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Description).HasMaxLength(500);
        
        builder.Property(b => b.City).HasMaxLength(100);
        builder.Property(b => b.AddressAr).HasMaxLength(500);
        builder.Property(b => b.AddressEn).HasMaxLength(500);
        builder.Property(b => b.PostalCode).HasMaxLength(20);
        
        builder.Property(b => b.PhoneNumber).HasMaxLength(20);
        builder.Property(b => b.Email).HasMaxLength(200);
        builder.Property(b => b.Fax).HasMaxLength(20);
        
        builder.Property(b => b.TimeZone).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(10);
        builder.Property(b => b.Language).HasMaxLength(10);
        builder.Property(b => b.WorkingDays).HasMaxLength(100);
        
        builder.Property(b => b.Country)
            .IsRequired()
            .HasConversion<int>();

        // Relationships
        builder.HasOne(b => b.Organization)
            .WithMany(o => o.Branches)
            .HasForeignKey(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BranchManager)
            .WithMany()
            .HasForeignKey(b => b.BranchManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Departments)
            .WithOne(d => d.Branch)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Employees)
            .WithOne(e => e.Branch)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
