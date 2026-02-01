using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeRequestOptionConfiguration : IEntityTypeConfiguration<EmployeeRequestOption>
{
    public void Configure(EntityTypeBuilder<EmployeeRequestOption> builder)
    {
        builder.ToTable("EmployeeRequestOptions", "Requests");

        builder.Property(o => o.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .HasMaxLength(1000);

        builder.Property(o => o.SortOrder)
            .HasDefaultValue(1);

        builder.HasIndex(o => new { o.TenantId, o.RequestType, o.NameEn })
            .IsUnique();
    }
}
