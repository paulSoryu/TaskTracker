using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using WebApiTaskTracker.Business.Models.Auths;
using WebApiTaskTracker.Business.Services.Auths;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs;
using WebApiTaskTracker.WebApi.DTOs.Auths;

namespace WebApiTaskTracker.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth");

        group.MapPost("/register", Register)
            .WithValidation<RegisterRequest>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/login", Login)
            .WithValidation<LoginRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/confirm-email", ConfirmEmail)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        var lockedGroup = group.MapGroup("")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        lockedGroup.MapGet("/send-email-confirmation", SendEmailConfirmation)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/change-password", ChangePassword)
            .WithValidation<ChangePasswordRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/request-change-email", RequestChangeEmail)
            .WithValidation<ChangeEmailRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/confirm-change-email", ConfirmChangeEmail)
            .WithValidation<ConfirmChangeEmailRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapGet("/me", GetUserInfo);

        lockedGroup.MapPost("/logout", Logout);

        lockedGroup.MapDelete("/delete-account", DeleteAccount)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    }

    private static async Task<Results<Ok<UserInfoView>, ProblemHttpResult>> GetUserInfo(ClaimsPrincipal userPrincipal, IAuthService authService)
    {
        Result<UserInfoView> result = await authService.GetCurrentUserInfoAsync(userPrincipal);
        return result.ToTypedHttpResult();
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

    private static async Task<Results<NoContent, ProblemHttpResult>> ConfirmEmail([AsParameters]ConfirmEmailRequest dto, IAuthService authService)
    {
        Result result = await authService.ConfirmEmailAsync(dto.UserId, dto.Token);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> SendEmailConfirmation(ClaimsPrincipal userPrincipal, IAuthService authService)
    {
        Result result = await authService.SendEmailConfirmationAsync(userPrincipal);
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