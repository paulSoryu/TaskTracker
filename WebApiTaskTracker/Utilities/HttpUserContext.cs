using System.Security.Claims;

namespace WebApiTaskTracker.Utilities;

public interface IUserContext
{
    Guid CurrentUserId { get; }
    bool IsAuthenticated { get; }
}

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CurrentUserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdString, out var guid))
            {
                return guid;
            }

            return Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}