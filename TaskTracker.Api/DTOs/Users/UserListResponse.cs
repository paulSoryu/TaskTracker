namespace TaskTracker.Api.DTOs.Users;

public record UserListResponse(
    Guid Id,
    string Email,
    bool IsEmailConfirmed,
    DateTime CreatedAt,
    DateTime LastOnlineTime,
    bool IsAdmin,
    DateTimeOffset? LockoutEnd,
    int TaskCount,
    int CompletedTaskCount,
    int CategoryCount
);
