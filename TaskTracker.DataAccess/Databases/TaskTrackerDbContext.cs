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

        // UserEntity doesn't have a query filter. For AuthEndpoints, they are using ClaimsPrincipal to get current user.
        // And for AdminEndpoints, they handle all users in the system, so we would need to write .IgnoreQueryFilters() everywhere
        
        // Still, when accessing tasks and categories of other users (to count them) we need to write .IgnoreQueryFilters()
        // This is more secure as it is better to be forced to remove filter when you really need it, as opposed to just forgetting to filter entities by current user 
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
