using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiTaskTracker.DataAccess.Configurations;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.DataAccess.Interfaces;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.DataAccess.Databases;

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
