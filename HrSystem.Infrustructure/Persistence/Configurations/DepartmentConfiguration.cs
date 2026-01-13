using HrSystem.Domain.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments", "Employee");

        builder.Property(d => d.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(d => d.NameEn).IsRequired().HasMaxLength(200);

        builder.HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ParentDepartment)
            .WithMany(d => d.SubDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
