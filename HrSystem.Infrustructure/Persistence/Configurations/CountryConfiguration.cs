using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries", "Organization");

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Currency)
            .HasMaxLength(3);

        builder.Property(x => x.TimeZone)
            .HasMaxLength(100);

        builder.Property(x => x.PhoneCode)
            .HasMaxLength(10);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(1);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasMany(x => x.Branches)
            .WithOne(b => b.Country)
            .HasForeignKey(b => b.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
