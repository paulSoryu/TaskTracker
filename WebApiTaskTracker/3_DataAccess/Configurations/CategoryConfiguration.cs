using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Drawing;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.DataAccess.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(p => p.Id);

        builder.HasOne(c => c.User)
               .WithMany(u => u.Categories)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Title)
               .IsRequired()
               .HasMaxLength(CategoryConstraints.TitleMaxLength);

        builder.HasIndex(c => new { c.UserId, c.Title })
               .IsUnique();

        builder.Property(p => p.Colour)
               .IsRequired()
               .HasMaxLength(9);
    }
}