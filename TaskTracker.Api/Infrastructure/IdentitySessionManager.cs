using FluentResults;
using Microsoft.AspNetCore.Identity;
using TaskTracker.Business.FluentErrors;
using TaskTracker.Business.Services.Identity;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Api.Infrastructure;

// This class is used to encapsulate methods of SignInManager which require UserEntity to operate, so that AuthEndpoints doesn't know anything about entities from DataAccesss layer
// It also encapsulates SignInManager itself
public class IdentitySessionManager(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager) : IIdentitySessionManager
{
    public async Task<Result> PasswordSignInAsync(string email, string password, bool isPersistent)
    {
        if (await userManager.FindByEmailAsync(email) == null)
            return Result.Fail(new UserNotFoundError(email));

        var signInResult = await signInManager.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure: false);

        if (signInResult.IsLockedOut)
            return Result.Fail(new ValidationError("Account is locked out."));
        if (!signInResult.Succeeded)
            return Result.Fail(new ValidationError("Invalid password."));

        return Result.Ok();
    }

    public async Task SignInAsync(string userId, bool isPersistent)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null) await signInManager.SignInAsync(user, isPersistent);
    }

    public async Task RefreshSignInAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null) 
            await signInManager.RefreshSignInAsync(user);
    }

    public async Task SignOutAsync()
    {
        await signInManager.SignOutAsync();
    }
}
