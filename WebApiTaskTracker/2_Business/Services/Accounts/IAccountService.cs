using Microsoft.AspNetCore.Identity;

namespace WebApiTaskTracker.Business.Services.Accounts
{
    public interface IAccountService
    {
        Task<IdentityResult> DeleteAccountAsync(string userId);
    }
}
