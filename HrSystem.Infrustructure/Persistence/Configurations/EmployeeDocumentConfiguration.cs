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

        builder.HasOne(ed => ed.DocumentType)
            .WithMany(dt => dt.Documents)
            .HasForeignKey(ed => ed.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ed => ed.DocumentTypeId);

        builder.HasOne(ed => ed.Employee)
            .WithMany(e => e.Documents)
            .HasForeignKey(ed => ed.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
