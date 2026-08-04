namespace WebApiTaskTracker.Business.Models.Auths;

public record UserInfoView
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public bool? IsEmailConfirmed { get; set; }
}
