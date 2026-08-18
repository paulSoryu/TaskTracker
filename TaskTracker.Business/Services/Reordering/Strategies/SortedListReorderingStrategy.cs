using FluentResults;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Business.Extensions;
using TaskTracker.DataAccess.Databases;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering.Strategies;

public class SortedListReorderingStrategy<TEntity>(TaskTrackerDbContext db) : IReorderingStrategy<TEntity>
    where TEntity : class, IOrderable
{
    public async Task<Result> MoveAsync(TEntity entity, TEntity targetEntity, ReorderingOptions<TEntity> options)
    {
        var allEntities = await options.ApplySorting(db.Set<TEntity>()).ToListAsync();

        int newPos = allEntities.IndexOf(targetEntity);

        allEntities.Remove(entity);
        allEntities.Insert(newPos, entity);

        await db.ResetOrderAsync(allEntities, options.IsDescending);
        return Result.Ok();
    }

    public async Task<Result> InsertAsync(
        TEntity newEntity, TEntity? targetEntity, int currentCount, ReorderingOptions<TEntity> options)
    {
        var allEntities = await options.ApplySorting(db.Set<TEntity>()).ToListAsync();

        int index = targetEntity != null
            ? allEntities.IndexOf(targetEntity)
            : allEntities.Count;

        allEntities.Insert(index, newEntity);
        db.Set<TEntity>().Add(newEntity);

        await db.ResetOrderAsync(allEntities, options.IsDescending);
        return Result.Ok();
    }
}
