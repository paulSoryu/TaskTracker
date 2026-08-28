using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.DTOs.Auths;
using TaskTracker.Api.DTOs.Tasks;
using TaskTracker.Api.Extensions;
using TaskTracker.Business.Models.Auths;
using TaskTracker.Business.Services.Auths;
using TaskTracker.Business.Services.Identity;
using TaskTracker.Business.Services.Users;

namespace TaskTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth");

        var lockedGroup = group.MapGroup("")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/register", Register)
            .WithValidation<RegisterRequest>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/login", Login)
            .WithValidation<LoginRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/logout", Logout);

        lockedGroup.MapPost("/request-email-confirmation", RequestEmailConfirmation)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/confirm-email", ConfirmEmail)
            .WithValidation<ConfirmEmailRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/request-email-change", RequestChangeEmail)
            .WithValidation<ChangeEmailRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/change-email", ChangeEmail)
            .WithValidation<ConfirmChangeEmailRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/change-password", ChangePassword)
            .WithValidation<ChangePasswordRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapGet("/me", GetUserInfo);

        lockedGroup.MapDelete("/delete-account", DeleteAccount)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Register(RegisterRequest request, IUserCoordinator userCoordinator)
    {
        Result result = await userCoordinator.RegisterAndCreateDefaultDataAsync(request.Email, request.Password);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Login(LoginRequest request, IIdentitySessionManager sessionManager)
    {
        Result result = await sessionManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe);
        return result.ToTypedHttpResult();
    }

    private static async Task<NoContent> Logout(IIdentitySessionManager sessionManager)
    {
        await sessionManager.SignOutAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RequestEmailConfirmation(ClaimsPrincipal user, IUserCoordinator userCoordinator)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await userCoordinator.SendEmailConfirmationLetterAsync(email);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ConfirmEmail([AsParameters]ConfirmEmailRequest request, IAuthService authService)
    {
        Result result = await authService.ConfirmEmailFromTokenAsync(request.UserId, request.EncodedToken);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RequestChangeEmail(ChangeEmailRequest request, ClaimsPrincipal user, IUserCoordinator userCoordinator)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await userCoordinator.SendEmailChangeLetterAsync(email, request.NewEmail, request.Password);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ChangeEmail([AsParameters] ConfirmChangeEmailRequest request, ClaimsPrincipal user, IAuthService authService, IIdentitySessionManager sessionManager)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await authService.ChangeEmailFromTokenAsync(email, request.NewEmail, request.EncodedToken);
        if (result.IsSuccess)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await sessionManager.RefreshSignInAsync(userId);
        }

        return result.ToTypedHttpResult();
    }
    private static async Task<Results<NoContent, ProblemHttpResult>> ChangePassword(ChangePasswordRequest request, ClaimsPrincipal user, IUserService userService)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await userService.UpdatePasswordAsync(email, request.CurrentPassword, request.NewPassword);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<Ok<UserInfoResponse>, ProblemHttpResult>> GetUserInfo(ClaimsPrincipal user, IUserService userService)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        Result<UserInfoView> result = await userService.GetInfoByIdAsync(Guid.Parse(id));
        Result<UserInfoResponse> response = result.Map(user => user.Adapt<UserInfoResponse>());
        return response.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAccount(ClaimsPrincipal user, string password, IUserCoordinator userCoordinator, IIdentitySessionManager sessionManager)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await userCoordinator.DeleteUserAndDataAsync(email, password);
        if (result.IsSuccess)
            await sessionManager.SignOutAsync();

        return result.ToTypedHttpResult();
    }
}