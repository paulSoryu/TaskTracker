using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Business.Extensions;
using TaskTracker.Business.FluentErrors;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Auths;
using TaskTracker.Business.Models.Users;
using TaskTracker.DataAccess.Databases;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Services.Users;

public class UserService(TaskTrackerDbContext db, UserManager<UserEntity> userManager) : IUserService
{
    public async Task<PagedResult<UserView>> GetAllAsync(FilterUsersQuery filterQuery, SortUsersQuery sortQuery, PaginateUsersQuery paginateQuery)
    {
        var baseQuery = db.Users
            .AsNoTracking()
            .ApplyFilter(filterQuery);

        var totalCount = await baseQuery.CountAsync();

        if (totalCount == 0)
            return new PagedResult<UserView>(new List<UserView>(), 0);

        var pagedData = await baseQuery
            .IgnoreQueryFilters()
            .ApplySorting(sortQuery)
            .ApplyPagination(paginateQuery)
            .ProjectToType<UserView>()
            .AsSplitQuery()
            .ToListAsync();

        // This is needed because Mapster couldn't map UserRoles.Any() to bool IsAdmin, maybe there's some other solution, but I couldn't find it
        // Maybe we should load UserRoles into UserView, and then map it into IsAdmin in UserListResponse
        if (pagedData.Any())
        {
            var pagedUserIds = pagedData.Select(u => u.Id).ToList();

            var activeAdminIds = await db.UserRoles
                .Where(ur => pagedUserIds.Contains(ur.UserId))
                .Select(ur => ur.UserId)
                .ToListAsync();

            var adminHashSet = activeAdminIds.ToHashSet();

            foreach (var userView in pagedData)
            {
                userView.IsAdmin = adminHashSet.Contains(userView.Id);
            }
        }

        return new PagedResult<UserView>(pagedData, totalCount);
    }

    public async Task<Result<UserView>> GetByIdAsync(Guid id)
    {
        var user = await db.Users
            .Where(u => u.Id == id)
            .ProjectToType<UserView>()
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        if (user == null)
            return Result.Fail(new NotFoundError("User", id));

        var response = user.Adapt<UserView>();

        return Result.Ok(response);
    }

    public async Task<Result<UserInfoView>> GetInfoByIdAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
            return Result.Fail(new NotFoundError("User", id));

        var response = user.Adapt<UserInfoView>();

        response.IsAdmin = await userManager.IsInRoleAsync(user, "Admin");

        return Result.Ok(response);
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

    public async Task<Result> AssignAdminRoleAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Fail(new NotFoundError("User", userId));

        if (await userManager.IsInRoleAsync(user, "Admin"))
            return Result.Fail(new ValidationError("This user already has this role"));

        var addRoleResult = await userManager.AddToRoleAsync(user, "Admin");

        if (!addRoleResult.Succeeded)
            return Result.Fail(new IdentityValidationError($"Couldn't add Admin role to user {user.Email}", addRoleResult.Errors));

        var updateSecurityResult = await userManager.UpdateSecurityStampAsync(user);

        return updateSecurityResult.Succeeded
            ? Result.Ok()
            : Result.Fail(new IdentityValidationError($"Couldn't update security stamp, ask user {user.Email} to log in manually", updateSecurityResult.Errors));
    }

    public async Task<Result> RemoveAdminRoleAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Fail(new NotFoundError("User", userId));

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            var admins = await userManager.GetUsersInRoleAsync("Admin");

            if (admins.Count <= 1)
                return Result.Fail(new ValidationError("Can't remove Admin role from the last admin in the system"));
        }

        var identityResult = await userManager.RemoveFromRoleAsync(user, "Admin");

        return identityResult.Succeeded
            ? Result.Ok()
            : Result.Fail(new IdentityValidationError($"Couldn't strip {user.Email} of Admin rights", identityResult.Errors));
    }

    public async Task<Result> BlockUserAsync(Guid userId, DateOnly? until)
    {
        if (until < DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Fail(new ValidationError("BlockDate must be today or in the future"));

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Fail(new NotFoundError("User", userId));

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            var admins = await userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
                return Result.Fail(new ValidationError("Can't block the only user with Admin role in the system"));
        }

        DateTimeOffset lockoutEndDate = until == null 
            ? DateTimeOffset.UtcNow.AddYears(200)
            : until.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var setLockoutResult = await userManager.SetLockoutEndDateAsync(user, lockoutEndDate);

        if (!setLockoutResult.Succeeded)
            return Result.Fail(new IdentityValidationError($"Couldn't block user {user.Email}", setLockoutResult.Errors));

        var updateSecurityResult = await userManager.UpdateSecurityStampAsync(user);

        return updateSecurityResult.Succeeded
            ? Result.Ok()
            : Result.Fail(new IdentityValidationError($"Couldn't update security stamp, ask user {user.Email} to log in manually", updateSecurityResult.Errors));
    }

    public async Task<Result> UnblockUserAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Fail(new NotFoundError("User", userId));

        var setLockoutResult = await userManager.SetLockoutEndDateAsync(user, null);

        if (!setLockoutResult.Succeeded)
            return Result.Fail(new IdentityValidationError($"Couldn't unblock user {user.Email}", setLockoutResult.Errors));

        var updateSecurityResult = await userManager.UpdateSecurityStampAsync(user);

        return updateSecurityResult.Succeeded
            ? Result.Ok()
            : Result.Fail(new IdentityValidationError($"Couldn't update security stamp, ask user {user.Email} to log in manually", updateSecurityResult.Errors));
    }

    public async Task<Result> UpdatePasswordAsync(string userEmail, string currentPassword, string newPassword)
    {
        var user = (await userManager.FindByEmailAsync(userEmail))!;

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        return result.ToFluentResult();
    }
}
