using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Models.Auths;
using WebApiTaskTracker.Business.Services.Auths;
using WebApiTaskTracker.Business.Services.Users;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs;
using WebApiTaskTracker.WebApi.DTOs.Auths;

namespace WebApiTaskTracker.WebApi.Endpoints;

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

        lockedGroup.MapPost("/send-email-confirmation", RequestEmailConfirmation)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/confirm-email", ConfirmEmail)
            .WithValidation<ConfirmEmailRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/request-change-email", RequestChangeEmail)
            .WithValidation<ChangeEmailRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        lockedGroup.MapPost("/confirm-change-email", ChangeEmail)
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

    private static async Task<Results<NoContent, ProblemHttpResult>> Login(LoginRequest request, IAuthService authService, SignInManager<UserEntity> signInManager)
    {                                                            // breaks the separation of layers, but otherwise we would need to get rid of ASP.NET Core entirely
        //Result result = await authService.LoginAsync(request.Email, request.Password, request.RememberMe);
        Result result = await authService.VerifyPasswordAsync(request.Email, request.Password);
        if (result.IsFailed)
            return result.ToTypedHttpResult();

        var signInResult = await signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, false);
        if (signInResult.IsLockedOut) return Result.Fail(new ValidationError("Account is locked out.")).ToTypedHttpResult();
        if (!signInResult.Succeeded) return Result.Fail(new ValidationError("Invalid password.")).ToTypedHttpResult();

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Logout(SignInManager<UserEntity> signInManager)
    {
        await signInManager.SignOutAsync();
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

    private static async Task<Results<NoContent, ProblemHttpResult>> ChangeEmail([AsParameters] ConfirmChangeEmailRequest request, ClaimsPrincipal user, IAuthService authService)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await authService.ChangeEmailFromTokenAsync(email, request.NewEmail, request.EncodedToken);
        return result.ToTypedHttpResult();
    }
    private static async Task<Results<NoContent, ProblemHttpResult>> ChangePassword(ChangePasswordRequest request, ClaimsPrincipal user, IUserService userService)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await userService.UpdatePasswordAsync(email, request.CurrentPassword, request.NewPassword);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<Ok<UserInfoView>, ProblemHttpResult>> GetUserInfo(ClaimsPrincipal user, IUserService userService)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        Result<UserInfoView> result = await userService.GetByIdAsync(id);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAccount(ClaimsPrincipal user, string password, IUserCoordinator userCoordinator, SignInManager<UserEntity> signInManager)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)!;

        Result result = await userCoordinator.DeleteUserAndDataAsync(email, password);
        if (result.IsSuccess)
            await signInManager.SignOutAsync();

        return result.ToTypedHttpResult();
    }
}