using Microsoft.AspNetCore.Identity;
using System.Collections;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.DataAccess.Entities;

// Most of the properties are inherited from IdentityUser<Guid> class, which includes Id, UserName, Email, PasswordHash, etc.
// This breaks single responsibility principle (better to use IdentityUser only for authentication-related properties),
// but we can live with it for now. We can always create a separate UserProfile entity if we need to add more properties in the future.
public class UserEntity : IdentityUser<Guid>, IAuditable
{
    public DateTime CreatedAt { get; set; }
    public DateTime LastOnlineTime { get; set; }

    public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = [];

    public List<CategoryEntity> Categories { get; set; } = [];
    public List<TaskEntity> Tasks { get; set; } = [];
}
