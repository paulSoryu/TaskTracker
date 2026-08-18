namespace TaskTracker.Api.DTOs.Auths;


public record UserInfoResponse
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public bool? IsEmailConfirmed { get; set; }
}