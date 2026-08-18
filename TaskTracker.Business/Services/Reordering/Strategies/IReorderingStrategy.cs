using FluentResults;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering.Strategies;

public interface IReorderingStrategy<TEntity> where TEntity : class, IOrderable
{
    Task<Result> MoveAsync(TEntity entity, TEntity targetEntity, ReorderingOptions<TEntity> options);

    Task<Result> InsertAsync(
        TEntity newEntity,
        TEntity? targetEntity,
        int currentCount,
        ReorderingOptions<TEntity> options);
}
