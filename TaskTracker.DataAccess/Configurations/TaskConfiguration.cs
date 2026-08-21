using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.DataAccess.Entities;
using TaskTracker.Shared.Constants;

namespace TaskTracker.DataAccess.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(p => p.Id);

        builder.HasOne(t => t.User)
               .WithMany(u => u.Tasks)
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Restrict); // Prevents deletion of a user if they have associated tasks. You have to delete the tasks manually before deleting the user.

        builder.HasOne(t => t.Category)
               .WithMany(c => c.Tasks)
               .HasForeignKey(t => t.CategoryId)
               .OnDelete(DeleteBehavior.SetNull); // Sets the CategoryId to null if the associated category is deleted

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

        // This would be nice for additional security, but it can't be used as easily with ExecuteUpdateAsync.
        // And even usual SaveChangesAsync will throw an exception if you try to update the Position of a task to a value that already exists for another task of the same user. So, this index is commented out for now.
        //builder.HasIndex(c => new { c.UserId, c.Position })
        //       .IsUnique();
    }
}