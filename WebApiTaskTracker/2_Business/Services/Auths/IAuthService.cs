using FluentResults;
using System.Security.Claims;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Services.Auths;

public interface IAuthService
{
    Task<Result<Guid>> RegisterAsync(string password, UserEntity user);
    Task<Result> VerifyPasswordAsync(UserEntity user, string password);
    Task<Result> VerifyPasswordAsync(string email, string password);
    Task<Result<string>> GenerateConfirmEmailTokenAsync(UserEntity user);
    Task<Result> ConfirmEmailFromTokenAsync(string userId, string encodedToken);
    Task<Result<string>> GenerateChangeEmailTokenAsync(UserEntity user, string newEmail);
    Task<Result> ChangeEmailFromTokenAsync(string currentEmail, string newEmail, string encodedToken);
}