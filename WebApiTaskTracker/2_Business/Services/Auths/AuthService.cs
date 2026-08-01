namespace WebApiTaskTracker.Business.Services.Auths;

using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;

public class AuthService(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, TaskTrackerDbContext db) : IAuthService
{
    public async Task<IdentityResult> RegisterAsync(string email, string password)
    {
        var user = new UserEntity { UserName = email, Email = email };

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return result;

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

            return IdentityResult.Success;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SignInResult> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return SignInResult.Failed;

        return await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        return await userManager.ConfirmEmailAsync(user, token);
    }

    public async Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "Unauthorized." });

        return await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<(IdentityResult, string?)> RequestChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return (IdentityResult.Failed(new IdentityError { Description = "Unauthorized." }), null);

        var token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        return (IdentityResult.Success, token);
    }

    public async Task<IdentityResult> ConfirmChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail, string token)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "Unauthorized." });

        var result = await userManager.ChangeEmailAsync(user, newEmail, token);
        if (result.Succeeded)
        {
            await userManager.SetUserNameAsync(user, newEmail);
            await signInManager.RefreshSignInAsync(user);
        }
        return result;
    }

    public async Task<IdentityResult> DeleteAccountAsync(ClaimsPrincipal userPrincipal)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "Unauthorized." });

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
                return result;
            }

            await transaction.CommitAsync();

            await signInManager.SignOutAsync();

            return IdentityResult.Success;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }
}