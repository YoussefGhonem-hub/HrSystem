using HrSystem.Domain.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations;

public class VacationTypeConfiguration : IEntityTypeConfiguration<VacationType>
{
    public void Configure(EntityTypeBuilder<VacationType> builder)
    {
        builder.ToTable("VacationTypes", "Requests");

        builder.Property(v => v.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(v => v.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Description).HasMaxLength(500);

        builder.HasIndex(v => new { v.TenantId, v.NameEn }).IsUnique();
    }
}

public class OvertimeTypeConfiguration : IEntityTypeConfiguration<OvertimeType>
{
    public void Configure(EntityTypeBuilder<OvertimeType> builder)
    {
        builder.ToTable("OvertimeTypes", "Requests");

        builder.Property(o => o.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(o => o.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Description).HasMaxLength(500);
        builder.Property(o => o.DefaultMultiplier).HasPrecision(4, 2);

        builder.HasIndex(o => new { o.TenantId, o.NameEn }).IsUnique();
    }
}

public class TrainingTypeConfiguration : IEntityTypeConfiguration<TrainingType>
{
    public void Configure(EntityTypeBuilder<TrainingType> builder)
    {
        builder.ToTable("TrainingTypes", "Requests");

        builder.Property(t => t.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(t => t.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(500);

        builder.HasIndex(t => new { t.TenantId, t.NameEn }).IsUnique();
    }
}

public class MiscellaneousTypeConfiguration : IEntityTypeConfiguration<MiscellaneousType>
{
    public void Configure(EntityTypeBuilder<MiscellaneousType> builder)
    {
        builder.ToTable("MiscellaneousTypes", "Requests");

        builder.Property(m => m.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(m => m.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(500);

        builder.HasIndex(m => new { m.TenantId, m.NameEn }).IsUnique();
    }
}

public class PersonalTypeConfiguration : IEntityTypeConfiguration<PersonalType>
{
    public void Configure(EntityTypeBuilder<PersonalType> builder)
    {
        builder.ToTable("PersonalTypes", "Requests");

        builder.Property(p => p.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(p => p.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(500);

        builder.HasIndex(p => new { p.TenantId, p.NameEn }).IsUnique();
    }
}

public class FeedbackTypeConfiguration : IEntityTypeConfiguration<FeedbackType>
{
    public void Configure(EntityTypeBuilder<FeedbackType> builder)
    {
        builder.ToTable("FeedbackTypes", "Requests");

        builder.Property(f => f.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(f => f.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Description).HasMaxLength(500);

        builder.HasIndex(f => new { f.TenantId, f.NameEn }).IsUnique();
    }
}
