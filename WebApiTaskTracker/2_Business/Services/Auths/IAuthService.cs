using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace WebApiTaskTracker.Business.Services.Auths;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(string email, string password);
    Task<SignInResult> LoginAsync(string email, string password);
    Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
    Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword);
    Task<(IdentityResult, string?)> RequestChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail);
    Task<IdentityResult> ConfirmChangeEmailAsync(ClaimsPrincipal userPrincipal, string newEmail, string token);
    Task<IdentityResult> DeleteAccountAsync(ClaimsPrincipal userPrincipal);
    Task LogoutAsync();
}