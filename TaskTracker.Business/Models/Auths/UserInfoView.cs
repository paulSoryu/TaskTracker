namespace TaskTracker.Business.Models.Auths;

public record UserInfoView(
    Guid Id,
    string Email,
    bool IsEmailConfirmed,
    DateTime CreatedAt
)
{
    public bool IsAdmin { get; set;  }
}