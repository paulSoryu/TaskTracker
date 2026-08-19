using FluentResults;
using Microsoft.AspNetCore.Identity;
using TaskTracker.Business.Extensions;
using TaskTracker.Business.Models.Auths;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Services.Users;

public class UserService(UserManager<UserEntity> userManager) : IUserService
{
    public async Task<Result> AssignRoleAsync(string userId, string roleName)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> BlockUserAsync(string userId, DateTimeOffset? until)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> UnblockUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    // this CreateAsync method doesn't write anything into DB as ASP.NET Identity already does this in RegisterAsync
    // but if we ever remove Identity, writing into DB should be here, and password hashing should be in RegisterAsync
    // it is also async just for the sake of consistency and easier changes later on
    public async Task<Result<UserEntity>> CreateAsync(string email)
    {
        var user = new UserEntity
        {
            UserName = email,
            Email = email
        };

        return Result.Ok(user);
    }

    public async Task<Result> DeleteAsync(string userId)
    {
        var user = (await userManager.FindByIdAsync(userId))!;
        var result = await userManager.DeleteAsync(user);
        return result.ToFluentResult();
    }

    public async Task<Result<List<UserInfoView>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Result<UserInfoView>> GetByIdAsync(string userId)
    {
        var user = (await userManager.FindByIdAsync(userId))!;

        return Result.Ok(new UserInfoView
        {
            UserId = user.Id,
            Email = user.Email,
            IsEmailConfirmed = user.EmailConfirmed
        });
    }

    public async Task<Result> UpdatePasswordAsync(string userEmail, string currentPassword, string newPassword)
    {
        var user = (await userManager.FindByEmailAsync(userEmail))!;

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        return result.ToFluentResult();
    }
}
