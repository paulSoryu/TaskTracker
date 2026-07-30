using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApiTaskTracker.Business.Services.Accounts;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.WebApi.Endpoints;


public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accountGroup = endpoints.MapGroup("/api/account");

        accountGroup.MapIdentityApi<UserEntity>();

        var protectedGroup = accountGroup.MapGroup("/").RequireAuthorization();

        protectedGroup.MapPost("/logout", Logout);
        protectedGroup.MapDelete("/delete", Delete);
    }

    // This method handles the logout process for a user. It removes the refresh token associated with the user's account, effectively logging them out of the application.
    // But it does not invalidate the access token, which may still be valid until it expires. Therefore, the user may still have access to protected resources until the access token expires.
    // Access tokens could only be invalidated on the frontend by removing them from the client-side storage (e.g., localStorage, sessionStorage, or cookies) after the logout process is completed.
    private static async Task<Results<Ok<string>, UnauthorizedHttpResult>> Logout(ClaimsPrincipal userPrincipal, UserManager<UserEntity> userManager)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return TypedResults.Unauthorized();

        await userManager.RemoveAuthenticationTokenAsync(
            user,
            "[AspNetCoreIdentityBearerToken]",
            "refresh_token");

        return TypedResults.Ok("Logout successful. Refresh token is deleted.");
    }

    private static async Task<Results<Ok<string>, UnauthorizedHttpResult, BadRequest<IEnumerable<IdentityError>>>> Delete(
        ClaimsPrincipal userPrincipal,
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IAccountService accountService)
    {
        var userId = userManager.GetUserId(userPrincipal);
        if (userId == null)
            return TypedResults.Unauthorized();

        var result = await accountService.DeleteAccountAsync(userId);

        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Errors);

        await signInManager.SignOutAsync();

        return TypedResults.Ok("Account successfully deleted.");
    }
}
