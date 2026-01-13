using HrSystem.Domain.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasIndex(e => e.EmployeeCode).IsUnique();
        builder.HasIndex(e => e.NationalId).IsUnique();
        builder.HasIndex(e => e.Email);

        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.FirstNameAr).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastNameAr).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FirstNameEn).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastNameEn).IsRequired().HasMaxLength(100);
        builder.Property(e => e.NationalId).IsRequired().HasMaxLength(14);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);

        // Relationships
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.JobTitle)
            .WithMany(j => j.Employees)
            .HasForeignKey(e => e.JobTitleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DirectManager)
            .WithMany(e => e.Subordinates)
            .HasForeignKey(e => e.DirectManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
