namespace WebApiTaskTracker.Business.Services.Auths;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;

public class AuthService(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, TaskTrackerDbContext db) : IAuthService
{
    public async Task<IdentityResult> RegisterAsync(string email, string password)
    {
        var user = new UserEntity { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            // Add default tasks and categories
            Guid[] guids = [Guid.NewGuid() , Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
            
            db.Categories.Add(new CategoryEntity { Id = guids[0], Title = "Work", Colour = "", UserId = user.Id });
            db.Categories.Add(new CategoryEntity { Id = guids[1], Title = "Personal", Colour = "", UserId = user.Id });
            db.Categories.Add(new CategoryEntity { Id = guids[2], Title = "Errands", Colour = "", UserId = user.Id });
            db.Categories.Add(new CategoryEntity { Id = guids[3], Title = "Health", Colour = "", UserId = user.Id });
            db.Categories.Add(new CategoryEntity { Id = guids[4], Title = "Other", Colour = "", UserId = user.Id });

            db.Tasks.Add(new TaskEntity { Title = "Prepare quarterly report", UserId = user.Id, CategoryId = guids[0] });
            db.Tasks.Add(new TaskEntity { Title = "Book dentist appointment", UserId = user.Id, CategoryId = guids[1] });
            db.Tasks.Add(new TaskEntity { Title = "Renew apartment insurance", UserId = user.Id, CategoryId = guids[2] });
            db.Tasks.Add(new TaskEntity { Title = "Grocery run", UserId = user.Id, CategoryId = guids[3] });
            db.Tasks.Add(new TaskEntity { Title = "Buy a present for my wife", UserId = user.Id, CategoryId = guids[4] });

            await db.SaveChangesAsync();
        }
        return result;
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