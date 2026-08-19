namespace TaskTracker.Api.DTOs.Users;

public record UserListResponse(
    Guid Id,
    string Email,
    bool IsEmailConfirmed,
    DateTime CreatedAt,
    DateTime LastOnlineTime,
    int TaskCount,
    int CompletedTaskCount,
    int CategoryCount
);
