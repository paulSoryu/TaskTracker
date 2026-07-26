using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApiTaskTracker.Data.Entities;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Data.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(p => p.Id);

        builder.HasOne(t => t.User)
               .WithMany(u => u.Tasks)
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Category)
               .WithMany(c => c.Tasks)
               .HasForeignKey(t => t.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.Title)
               .IsRequired()
               .HasMaxLength(TaskConstraints.TitleMaxLength);

        builder.Property(p => p.Description)
               .HasMaxLength(TaskConstraints.DescriptionMaxLength);

        builder.Property(p => p.CreatedAt)
               .IsRequired();

        builder.Property(p => p.Priority)
               .IsRequired();

        builder.HasIndex(t => t.UserId);
    }
}