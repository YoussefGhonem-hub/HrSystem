using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSystem.Infrustructure.Persistence.Configurations.Performance;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("Goals", "Performance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TitleEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(1000);

        builder.Property(x => x.DescriptionEn)
            .HasMaxLength(1000);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.TargetDate)
            .IsRequired();

        builder.Property(x => x.CompletionDate);

        builder.Property(x => x.Progress)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CompletionNotes)
            .HasMaxLength(1000);

        // Foreign Keys
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany(s => s.Goals)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Priority)
            .WithMany(p => p.Goals)
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Milestones)
            .WithOne(m => m.Goal)
            .HasForeignKey(m => m.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
