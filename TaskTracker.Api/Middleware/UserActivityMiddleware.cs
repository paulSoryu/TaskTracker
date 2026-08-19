namespace TaskTracker.Api.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using TaskTracker.DataAccess.Entities;

public class UserActivityMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private const string CacheKeyPrefix = "UserActive_";
    private static readonly TimeSpan ActivityUpdateInterval = TimeSpan.FromMinutes(5);

    public async Task InvokeAsync(HttpContext context, UserManager<UserEntity> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = userManager.GetUserId(context.User);
            if (!string.IsNullOrEmpty(userId))
            {
                string cacheKey = $"{CacheKeyPrefix}{userId}";

                if (!cache.TryGetValue(cacheKey, out _))
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        user.LastOnlineTime = DateTime.UtcNow;
                        await userManager.UpdateAsync(user);

                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(ActivityUpdateInterval);

                        cache.Set(cacheKey, true, cacheOptions);
                    }
                }
            }
        }

        await next(context);
    }
}
