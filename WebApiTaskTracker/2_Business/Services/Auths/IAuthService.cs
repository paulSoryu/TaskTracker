using FluentResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApiTaskTracker.Business.Models.Auths;

namespace WebApiTaskTracker.Business.Services.Auths;

public interface IAuthService
{
    Task<Result> RegisterAsync(string email, string password);
    Task<Result> LoginAsync(string email, string password);
    Task<Result> ConfirmEmailAsync(string userId, string token);
    Task<Result> SendEmailConfirmationAsync(ClaimsPrincipal userPrincipal);
    Task<Result> ChangePasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword);
    Task<Result> RequestChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail);
    Task<Result> ConfirmChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail, string token);
    Task<Result<UserInfoView>> GetCurrentUserInfoAsync(ClaimsPrincipal principal);
    Task LogoutAsync();
    Task<Result> DeleteAccountAsync(ClaimsPrincipal userPrincipal, string password);
}