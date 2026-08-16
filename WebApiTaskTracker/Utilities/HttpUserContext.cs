using System.Security.Claims;

namespace WebApiTaskTracker.Utilities;

public interface IUserContext
{
    Guid CurrentUserId { get; }
    bool IsAuthenticated { get; }
}

public class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid CurrentUserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            var userIdString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdString, out var guid))
                return guid;

            return Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}