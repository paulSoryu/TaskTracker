namespace TaskTracker.Business.Services.UserActivity;

public interface IUserActivityTracker
{
    Task UpdateLastOnlineAsync(Guid userId);
}