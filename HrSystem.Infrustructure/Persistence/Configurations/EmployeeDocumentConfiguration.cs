using HrSystem.Domain.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments", "Employee");

        builder.Property(ed => ed.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(ed => ed.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(ed => ed.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(ed => ed.DocumentType);

        builder.HasOne(ed => ed.Employee)
            .WithMany(e => e.Documents)
            .HasForeignKey(ed => ed.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
