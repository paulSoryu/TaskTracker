using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly TaskTrackerDbContext _dbContext;
        private readonly UserManager<UserEntity> _userManager;

        public AccountService(TaskTrackerDbContext dbContext, UserManager<UserEntity> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IdentityResult> DeleteAccountAsync(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.Tasks
                    .Where(t => t.UserId == Guid.Parse(userId))
                    .ExecuteDeleteAsync();

                var result = await _userManager.DeleteAsync(appUser);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return result;
                }

                await transaction.CommitAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
    }
}
