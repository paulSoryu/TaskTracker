using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Business.Extensions;
using TaskTracker.Business.FluentErrors;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Auths;
using TaskTracker.Business.Models.Tasks;
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
        var user = (await userManager.FindByIdAsync(id))!;

        if (user == null)
            return Result.Fail(new NotFoundError("User", id));

        var response = user.Adapt<UserInfoView>();

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

    public async Task<Result> UpdatePasswordAsync(string userEmail, string currentPassword, string newPassword)
    {
        var user = (await userManager.FindByEmailAsync(userEmail))!;

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        return result.ToFluentResult();
    }
}
