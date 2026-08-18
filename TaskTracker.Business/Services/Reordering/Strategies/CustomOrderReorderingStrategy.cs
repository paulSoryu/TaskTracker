using FluentResults;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Business.Extensions;
using TaskTracker.Business.FluentErrors;
using TaskTracker.DataAccess.Databases;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering.Strategies;

public class CustomOrderReorderingStrategy<TEntity>(TaskTrackerDbContext db) : IReorderingStrategy<TEntity>
    where TEntity : class, IOrderable
{
    public async Task<Result> MoveAsync(TEntity entity, TEntity targetEntity, ReorderingOptions<TEntity> options)
    {
        int oldPos = entity.Position;
        int newPos = targetEntity.Position;

        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            await db.ReorderInRangeAsync<TEntity>(oldPos, newPos);

            await db.Set<TEntity>()
                .Where(e => e.Id == entity.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Position, newPos));

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ReorderingError(typeof(TEntity).Name, entity.Id, oldPos, newPos, ex.Message));
        }
    }

    public async Task<Result> InsertAsync(
        TEntity newEntity, TEntity? targetEntity, int currentCount, ReorderingOptions<TEntity> options)
    {
        int newPos = targetEntity != null
            ? targetEntity.Position + (options.IsDescending ? 1 : 0)
            : currentCount + 1;

        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            newEntity.Position = newPos;

            await db.ReorderInRangeAsync<TEntity>(currentCount + 1, newPos);

            db.Set<TEntity>().Add(newEntity);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ReorderingError(typeof(TEntity).Name, newEntity.Id, currentCount + 1, newPos, ex.Message));
        }
    }
}