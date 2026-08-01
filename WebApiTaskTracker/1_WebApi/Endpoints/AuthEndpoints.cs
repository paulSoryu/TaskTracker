using System.Security.Claims;
using WebApiTaskTracker.Business.Services.Auths;
using WebApiTaskTracker.WebApi.DTOs.Auth;

namespace WebApiTaskTracker.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapGet("/confirm-email", ConfirmEmail);
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

    private static async Task<IResult> Register(RegisterDto dto, IAuthService authService)
    {
        var result = await authService.RegisterAsync(dto.Email, dto.Password);
        return result.Succeeded ? Results.Ok("User registered.") : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> Login(LoginDto dto, IAuthService authService)
    {
        var result = await authService.LoginAsync(dto.Email, dto.Password);
        if (result.Succeeded) return Results.Ok("Logged in.");
        if (result.IsNotAllowed) return Results.BadRequest("Email not confirmed.");
        return Results.BadRequest("Invalid login attempt.");
    }

    private static async Task<IResult> ConfirmEmail(string userId, string token, IAuthService authService)
    {
        var result = await authService.ConfirmEmailAsync(userId, token);
        return result.Succeeded ? Results.Ok("Email confirmed.") : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> ChangePassword(ChangePasswordDto dto, ClaimsPrincipal user, IAuthService authService)
    {
        var result = await authService.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded ? Results.Ok("Password changed.") : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> RequestChangeEmail(RequestChangeEmailDto dto, ClaimsPrincipal user, IAuthService authService)
    {
        var (result, token) = await authService.RequestChangeEmailAsync(user, dto.NewEmail);
        return result.Succeeded ? Results.Ok(new { Message = "Token sent.", DebugToken = token }) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> ConfirmChangeEmail(ConfirmChangeEmailDto dto, ClaimsPrincipal user, IAuthService authService)
    {
        var result = await authService.ConfirmChangeEmailAsync(user, dto.NewEmail, dto.Token);
        return result.Succeeded ? Results.Ok("Email changed. Please verify it if required.") : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> Logout(IAuthService authService)
    {
        await authService.LogoutAsync();
        return Results.Ok("Logged out.");
    }

    private static async Task<IResult> DeleteAccount(ClaimsPrincipal user, IAuthService authService)
    {
        var result = await authService.DeleteAccountAsync(user);
        return result.Succeeded ? Results.Ok("Account deleted.") : Results.BadRequest(result.Errors);
    }
}