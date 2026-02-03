using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class BranchHolidayConfiguration : IEntityTypeConfiguration<BranchHoliday>
{
    public void Configure(EntityTypeBuilder<BranchHoliday> builder)
    {
        builder.ToTable("BranchHolidays", "Organization");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasOne(x => x.Branch)
            .WithMany(b => b.Holidays)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.BranchId);
        builder.HasIndex(x => new { x.BranchId, x.Date, x.Year });
    }
}
