using HrSystem.Domain.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class JobTitleConfiguration : IEntityTypeConfiguration<JobTitle>
{
    public void Configure(EntityTypeBuilder<JobTitle> builder)
    {
        builder.ToTable("JobTitles", "Employee");

        builder.Property(j => j.TitleAr).IsRequired().HasMaxLength(200);
        builder.Property(j => j.TitleEn).IsRequired().HasMaxLength(200);
        builder.Property(j => j.MinSalary).HasColumnType("decimal(18,2)");
        builder.Property(j => j.MaxSalary).HasColumnType("decimal(18,2)");
    }
}
