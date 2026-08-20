using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskTracker.DataAccess.Configurations;
using TaskTracker.DataAccess.Entities;
using TaskTracker.DataAccess.Interfaces;
using TaskTracker.Shared.Utilities;

namespace TaskTracker.DataAccess.Databases;

public class TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options, IUserContext userContext) : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());

        modelBuilder.Entity<CategoryEntity>()
            .HasQueryFilter(c => !userContext.IsAuthenticated || c.UserId == userContext.CurrentUserId);

        modelBuilder.Entity<TaskEntity>()
            .HasQueryFilter(t => !userContext.IsAuthenticated || t.UserId == userContext.CurrentUserId);

        // Breaks userManager.FindByIdAsync(userId) in AuthService.cs, so commented out for now
        //modelBuilder.Entity<UserEntity>()
        //    .HasQueryFilter(u => !_userContext.IsAuthenticated || u.Id == _userContext.CurrentUserId);

        // Connect Users to Roles to know who's admin and who's not
        modelBuilder.Entity<UserEntity>()
                    .HasMany(u => u.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<IAuditable>()
        .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            entry.Entity.CreatedAt = DateTime.UtcNow;
        }
    }
}
