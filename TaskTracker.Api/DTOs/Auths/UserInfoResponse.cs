namespace TaskTracker.Api.DTOs.Auths;


public record UserInfoResponse(
    Guid Id,
    string Email,
    bool IsEmailConfirmed,
    DateTime CreatedAt,
    bool IsAdmin
);