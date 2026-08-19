using FluentResults;

namespace TaskTracker.Business.Services.Identity;

// This interface is used as a contract with Api layer to inverse the dependency between layers, otherwise Business layer was dependant on Api layer due to Api layer requiring UserEntity for SignInManager
public interface IIdentitySessionManager
{
    Task<Result> PasswordSignInAsync(string email, string password, bool isPersistent);
    Task SignInAsync(string userId, bool isPersistent);
    Task RefreshSignInAsync(string userId);
    Task SignOutAsync();
}