using System.Security.Claims;

namespace WebApiTaskTracker.Utilities;

// Extension method to get the user ID from the ClaimsPrincipal
// It is similar to the GetUserId method in the UserManager class, but it is not doing any database calls, so it is more efficient.
// It is also more flexible, as it can be used in any context where a ClaimsPrincipal is available, not just in the context of a UserManager.
// And it returns a Guid instead of a string, which is more convenient for our application, as we are using Guid as the primary key for our users.
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw new UnauthorizedAccessException("User not authorized.");
        }

        return userId;
    }
}
