using HrSystem.Domain.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class UserBranchRoleConfiguration : IEntityTypeConfiguration<UserBranchRole>
{
    public void Configure(EntityTypeBuilder<UserBranchRole> builder)
    {
        builder.ToTable("UserBranchRoles", "security");

        builder.Property(x => x.RoleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.BranchId, x.RoleName })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(u => u.BranchRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Branch)
            .WithMany(b => b.UserBranchRoles)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
