namespace WebApiTaskTracker.Business.Services.Auths;

using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;

public class AuthService(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, TaskTrackerDbContext db) : IAuthService
{


    public async Task<Result<Guid>> RegisterAsync(string password, UserEntity user)
    {
        var identityResult = await userManager.CreateAsync(user, password);

        return identityResult.Succeeded
            ? Result.Ok(user.Id)
            : Result.Fail(new IdentityValidationError("Email or password is invalid.", identityResult.Errors));
    }

    public async Task<Result> VerifyPasswordAsync(UserEntity user, string password)
    {
        var result = await userManager.CheckPasswordAsync(user, password);

        return result
            ? Result.Ok()
            : Result.Fail(new ValidationError("Invalid Password"));
    }

    public async Task<Result> VerifyPasswordAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Fail(new ValidationError("Invalid email"));

        var result = await userManager.CheckPasswordAsync(user, password);

        return result
            ? Result.Ok()
            : Result.Fail(new ValidationError("Invalid Password"));
    }

    public async Task<Result<string>> GenerateConfirmEmailTokenAsync(UserEntity user)
    {
        if (user.EmailConfirmed)
            return Result.Fail(new ValidationError("Email already confirmed."));

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return Result.Ok(encodedToken);
    }

    // Ideally, we should separate email updating logic and move it into UserService, and this method should only verify the token, but it will do for now while we are using ASP.NET Identity
    public async Task<Result> ConfirmEmailFromTokenAsync(string userId, string encodedToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Fail(new ValidationError("Invalid user."));

        var decodedBytes = WebEncoders.Base64UrlDecode(encodedToken);
        var originalToken = Encoding.UTF8.GetString(decodedBytes);

        var result = await userManager.ConfirmEmailAsync(user, originalToken);
        return result.Succeeded
            ? Result.Ok()
            : Result.Fail(new IdentityValidationError("Invalid or expired token.", result.Errors));
    }

    public async Task<Result<string>> GenerateChangeEmailTokenAsync(UserEntity user, string newEmail)
    {
        if (newEmail == user.Email)
            return Result.Fail(new ValidationError("New email cannot be the same as the current email."));

        bool emailExists = await userManager.FindByEmailAsync(newEmail) != null;
        if (emailExists)
            return Result.Fail(new ValidationError("Email is already in use."));

        var token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return Result.Ok(encodedToken);
    }

    public async Task<Result> ChangeEmailFromTokenAsync(string currentEmail, string newEmail, string encodedToken)
    {
        var user = (await userManager.FindByEmailAsync(currentEmail))!;

        var decodedBytes = WebEncoders.Base64UrlDecode(encodedToken);
        var token = Encoding.UTF8.GetString(decodedBytes);

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var result = await userManager.ChangeEmailAsync(user, newEmail, token);
            if (!result.Succeeded)
                return Result.Fail(new IdentityValidationError($"Failed to change email from {currentEmail} to {newEmail}.", result.Errors));

            await userManager.SetUserNameAsync(user, newEmail);
            await signInManager.RefreshSignInAsync(user);

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("Failed to change email due to an internal database error.", ex));
        }
    }
}