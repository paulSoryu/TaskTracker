using TaskTracker.Business.Models.Enums;

namespace TaskTracker.Business.Models.Users;

public record SortUsersQuery(
    UserSortField SortBy,
    bool IsDescending
);
