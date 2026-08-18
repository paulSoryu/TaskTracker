namespace TaskTracker.Shared.Utilities;

public interface IUserContext
{
    Guid CurrentUserId { get; }
    bool IsAuthenticated { get; }
}
