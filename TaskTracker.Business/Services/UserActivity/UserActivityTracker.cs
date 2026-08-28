using Microsoft.EntityFrameworkCore;
using TaskTracker.DataAccess.Databases;

namespace TaskTracker.Business.Services.UserActivity;

public class UserActivityTracker(TaskTrackerDbContext dbContext) : IUserActivityTracker
{
    public async Task UpdateLastOnlineAsync(Guid userId)
    {
        await dbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastOnlineTime, DateTime.UtcNow));
    }
}
