using Microsoft.AspNetCore.Identity;

namespace WebApiTaskTracker.DataAccess.Entities;

// Most of the properties are inherited from IdentityUser<Guid> class, which includes Id, UserName, Email, PasswordHash, etc.
// This breaks single responsibility principle (better to useIdentityUser only for authentication-related properties),
// but we can live with it for now. We can always create a separate UserProfile entity if we need to add more properties in the future.
public class UserEntity : IdentityUser<Guid>
{
    public List<CategoryEntity> Categories { get; set; } = [];
    public List<TaskEntity> Tasks { get; set; } = [];
}
