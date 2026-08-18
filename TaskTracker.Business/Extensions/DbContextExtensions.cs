using Microsoft.EntityFrameworkCore;
using TaskTracker.DataAccess.Databases;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Extensions;

public static class DbContextExtensions
{
    public static async Task ReorderInRangeAsync<T>(this DbContext db, int start, int end)
        where T : class, IOrderable
    {
        if (start < end)    // Downshift
        {
            await db.Set<T>()
                .Where(t => t.Position > start && t.Position <= end)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position - 1));
        }
        else                // Upshift
        {
            await db.Set<T>()
                .Where(t => t.Position >= end && t.Position < start)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position + 1));
        }
    }

    public static async Task ResetOrderAsync<T>(this DbContext db, List<T> entities, bool isDescending)
        where T : class, IOrderable
    {
        if (isDescending)
            entities.Reverse();

        for (int i = 0; i < entities.Count; i++)
            entities[i].Position = i + 1;

        await db.SaveChangesAsync();
    }
}