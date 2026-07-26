using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiTaskTracker.DataAccess.Configurations;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.DataAccess.Databases;

public class TaskTrackerDbContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
{
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    private readonly IUserContext _userContext;

    public TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options, IUserContext userContext)
        : base(options)
    {
        _userContext = userContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());

        modelBuilder.Entity<CategoryEntity>()
            .HasQueryFilter(c => !_userContext.IsAuthenticated || c.UserId == _userContext.CurrentUserId);

        modelBuilder.Entity<TaskEntity>()
            .HasQueryFilter(t => !_userContext.IsAuthenticated || t.UserId == _userContext.CurrentUserId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>()
        .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            entry.Entity.CreatedAt = DateTime.UtcNow;
        }
    }
}
