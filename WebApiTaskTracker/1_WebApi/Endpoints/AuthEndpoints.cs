using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using WebApiTaskTracker.Business.Services.Auths;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs.Auth;

namespace WebApiTaskTracker.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapGet("/confirm-email", ConfirmEmail)
            .RequireAuthorization();
        group.MapPost("/change-password", ChangePassword)
            .RequireAuthorization();
        group.MapPost("/request-change-email", RequestChangeEmail)
            .RequireAuthorization();
        group.MapPost("/confirm-change-email", ConfirmChangeEmail)
            .RequireAuthorization();
        group.MapPost("/logout", Logout)
            .RequireAuthorization();
        group.MapDelete("/delete-account", DeleteAccount)
            .RequireAuthorization();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Register(RegisterRequest dto, IAuthService authService)
    {
        Result result = await authService.RegisterAsync(dto.Email, dto.Password);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Login(LoginRequest dto, IAuthService authService)
    {
        Result result = await authService.LoginAsync(dto.Email, dto.Password);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ConfirmEmail(string userId, string token, IAuthService authService)
    {
        Result result = await authService.ConfirmEmailAsync(userId, token);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ChangePassword(ChangePasswordRequest dto, ClaimsPrincipal user, IAuthService authService)
    {
        Result result = await authService.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<Ok<object>, ProblemHttpResult>> RequestChangeEmail(ChangeEmailRequest dto, ClaimsPrincipal user, IAuthService authService)
    {
        Result<string> result = await authService.RequestChangeEmailAsync(user, dto.NewEmail);

        Result<object> responseResult = result.Map(token => (object)new
        {
            Message = "Token sent.",
            DebugToken = token
        });

        return responseResult.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ConfirmChangeEmail(ConfirmChangeEmailRequest dto, ClaimsPrincipal user, IAuthService authService)
    {
        Result result = await authService.ConfirmChangeEmailAsync(user, dto.NewEmail, dto.Token);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAccount(ClaimsPrincipal user, IAuthService authService)
    {
        Result result = await authService.DeleteAccountAsync(user);
        return result.ToTypedHttpResult();
    }

    private static async Task<NoContent> Logout(IAuthService authService)
    {
        await authService.LogoutAsync();
        return TypedResults.NoContent();
    }
}