using FluentResults;
using WebApiTaskTracker.Business.Models.Auths;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Services.Users;

public interface IUserService
{
    // Data retrival
    Task<Result<UserInfoView>> GetByIdAsync(string userId);
    Task<Result<List<UserInfoView>>> GetAllAsync();

    // Lifecycle
    Task<Result<UserEntity>> CreateAsync(string email);
    Task<Result> DeleteAsync(string userId);

    // Account updates
    Task<Result> UpdatePasswordAsync(string userEmail, string currentPassword, string newPassword);

    // Admin functions
    Task<Result> AssignRoleAsync(string userId, string roleName);
    Task<Result> BlockUserAsync(string userId, DateTimeOffset? until);
}
