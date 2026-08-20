using TaskTracker.Business.Models.Categories;
using TaskTracker.Business.Models.Tasks;

namespace TaskTracker.Business.Models.Users;

public record UserView(
    Guid Id,
    string Email,
    bool IsEmailConfirmed,
    DateTime CreatedAt,
    DateTime LastOnlineTime,
    DateTimeOffset? LockoutEnd,

    IReadOnlyCollection<TaskView> Tasks,
    IReadOnlyCollection<CategoryView> Categories
)
{
    public bool IsAdmin { get; set; }
}