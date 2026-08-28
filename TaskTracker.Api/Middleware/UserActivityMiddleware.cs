namespace TaskTracker.Api.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskTracker.Business.Services.UserActivity;

public class UserActivityMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private const string CacheKeyPrefix = "UserActive_"; 
    private static readonly TimeSpan ActivityUpdateInterval = TimeSpan.FromMinutes(5);
    public async Task InvokeAsync(HttpContext context, IUserActivityTracker activityTracker)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            
            if (!string.IsNullOrEmpty(userId))
            {
                string cacheKey = $"{CacheKeyPrefix}{userId}";

                if (!cache.TryGetValue(cacheKey, out _))
                {
                    await activityTracker.UpdateLastOnlineAsync(Guid.Parse(userId));

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(ActivityUpdateInterval);

                    cache.Set(cacheKey, true, cacheOptions);
                }
            }
        }

        await next(context);
    }
}