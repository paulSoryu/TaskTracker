namespace TaskTracker.Api.DTOs.Users;


public record UserResponse(
    Guid Id,
    string Email,
    bool IsEmailConfirmed,
    DateTime CreatedAt,
    int TaskCount,
    int CompletedTaskCount,
    int CategoryCount
);
