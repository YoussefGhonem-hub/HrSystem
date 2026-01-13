using HrSystem.Domain.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Employee)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.OrganizationId);
        builder.HasIndex(u => u.EmployeeId);
    }
}
