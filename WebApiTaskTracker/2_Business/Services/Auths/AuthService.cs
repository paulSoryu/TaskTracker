namespace WebApiTaskTracker.Business.Services.Auths;

using FluentResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApiTaskTracker.Business.Extensions;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;

public class AuthService(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, TaskTrackerDbContext db) : IAuthService
{
    public async Task<Result> RegisterAsync(string email, string password)
    {
        var user = new UserEntity { UserName = email, Email = email };

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var identityResult = await userManager.CreateAsync(user, password);
            if (!identityResult.Succeeded)
                return Result.Fail(new IdentityValidationError("Email or password is invalid.", identityResult.Errors));

            Guid[] guids = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
            var defaultCategories = new List<CategoryEntity>
            {
                new() { Id = guids[0], Title = "Work", Colour = "#B5602D", UserId = user.Id },
                new() { Id = guids[1], Title = "Personal", Colour = "#9C4A5C", UserId = user.Id },
                new() { Id = guids[2], Title = "Errands", Colour = "#5B6E9C", UserId = user.Id },
                new() { Id = guids[3], Title = "Health", Colour = "#2F6F6B", UserId = user.Id },
                new() { Id = guids[4], Title = "Other", Colour = "#8A8577", UserId = user.Id }
            };
            var defaultTasks = new List<TaskEntity>
            {
                new() { Title = "Prepare quarterly report",  Description = "Pull Q2 numbers from the finance sheet, draft summary slides, send to review before Friday.",   Priority = TaskPriority.High,   DueDate = DateOnly.Parse("2026-08-04"), UserId = user.Id, CategoryId = guids[0] },
                new() { Title = "Book dentist appointment",  Description = "Call the clinic on Karl Marx ave, ask for a morning slot next week.",                           Priority = TaskPriority.Medium, DueDate = DateOnly.Parse("2026-09-25"), UserId = user.Id, CategoryId = guids[1] },
                new() { Title = "Renew apartment insurance", Description = "Compare two offers, pick the cheaper one with the same coverage, pay online.",                  Priority = TaskPriority.Low,    DueDate = DateOnly.Parse("2026-08-12"), UserId = user.Id, CategoryId = guids[2] },
                new() { Title = "Grocery run",               Description = "Milk, eggs, bread, coffee, something for Sunday dinner.",                                       Priority = TaskPriority.Low,    DueDate = null,                         UserId = user.Id, CategoryId = guids[3] },
                new() { Title = "Buy presents for kids",     Description = "Check 3 options, compare prices, and purchase the best gifts.",                                 Priority = TaskPriority.High,   DueDate = DateOnly.Parse("2026-09-01"), UserId = user.Id, CategoryId = guids[4] }
            };

            db.Categories.AddRange(defaultCategories);
            db.Tasks.AddRange(defaultTasks);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            await signInManager.SignInAsync(user, isPersistent: true);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Fail(new ExceptionalError("Registration failed due to an internal database error.", ex));
        }
    }

    public async Task<Result> LoginAsync(string email, string password)
    {
        var user = await (userManager.FindByEmailAsync(email))!;

        if (user == null)
            return Result.Fail(new ValidationError("Invalid email or password."));

        var signInResult = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);

        if (signInResult.IsLockedOut)  return Result.Fail(new ValidationError("Account is locked out."));
        if (signInResult.IsNotAllowed) return Result.Fail(new ValidationError("Login is not allowed. Check email confirmation."));
        if (!signInResult.Succeeded)   return Result.Fail(new ValidationError("Invalid email or password."));

        return Result.Ok();
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        var user = (await userManager.FindByIdAsync(userId))!;

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.ToFluentResult();
    }

    public async Task<Result> ChangePasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword)
    {
        var user = (await userManager.GetUserAsync(userPrincipal))!;

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.ToFluentResult();
    }

    public async Task<Result<string>> RequestChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail)
    {
        var user = (await userManager.GetUserAsync(userPrincipal))!;

        var token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        return Result.Ok(token);
    }

    public async Task<Result> ConfirmChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail, string token)
    {
        var user = (await userManager.GetUserAsync(userPrincipal))!;

        var result = await userManager.ChangeEmailAsync(user, newEmail, token);
        if (result.Succeeded)
        {
            await userManager.SetUserNameAsync(user, newEmail);
            await signInManager.RefreshSignInAsync(user);
        }
        return result.ToFluentResult();
    }

    public async Task<Result> DeleteAccountAsync(ClaimsPrincipal userPrincipal)
    {
        var user = (await userManager.GetUserAsync(userPrincipal))!;

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var userTasks = db.Tasks.Where(t => t.UserId == user.Id);
            var userCategories = db.Categories.Where(c => c.UserId == user.Id);

            db.Tasks.RemoveRange(userTasks);
            db.Categories.RemoveRange(userCategories);
            await db.SaveChangesAsync();

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return result.ToFluentResult();
            }

            await transaction.CommitAsync();
            await signInManager.SignOutAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Fail(new Error("Failed to delete account due to a server error.").CausedBy(ex));
        }
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }
}