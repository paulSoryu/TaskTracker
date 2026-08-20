using FluentResults;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Auths;
using TaskTracker.Business.Models.Users;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Services.Users;

public interface IUserService
{
    // Data retrival
    Task<PagedResult<UserView>> GetAllAsync(FilterUsersQuery filterQuery, SortUsersQuery sortQuery, PaginateUsersQuery paginateQuery);
    Task<Result<UserView>> GetByIdAsync(Guid userId);
    Task<Result<UserInfoView>> GetInfoByIdAsync(string id);

    // Lifecycle
    Task<Result<UserEntity>> CreateAsync(string email);
    Task<Result> DeleteAsync(string userId);

    // Account updates
    Task<Result> UpdatePasswordAsync(string userEmail, string currentPassword, string newPassword);

    // Admin functions
    Task<Result> AssignAdminRoleAsync(Guid userId);
    Task<Result> RemoveAdminRoleAsync(Guid userId);
    Task<Result> BlockUserAsync(Guid userId, DateOnly? until);
    Task<Result> UnblockUserAsync(Guid userId);
}
