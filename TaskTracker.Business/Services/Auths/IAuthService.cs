using FluentResults;
using System.Security.Claims;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Services.Auths;

public interface IAuthService
{
    Task<Result<Guid>> RegisterAsync(string password, UserEntity user);
    Task<Result> VerifyPasswordAsync(UserEntity user, string password);
    Task<Result> VerifyPasswordAsync(string email, string password);
    Task<Result<string>> GenerateConfirmEmailTokenAsync(UserEntity user);
    Task<Result> ConfirmEmailFromTokenAsync(string userId, string encodedToken);
    Task<Result<string>> GenerateChangeEmailTokenAsync(UserEntity user, string newEmail);
    Task<Result<UserEntity>> ChangeEmailFromTokenAsync(string currentEmail, string newEmail, string encodedToken);
}