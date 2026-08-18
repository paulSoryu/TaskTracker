using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering;

public class ReorderingOptions<TEntity> where TEntity : class, IOrderable
{
    public required bool IsDescending { get; init; }
    public required Func<IQueryable<TEntity>, IQueryable<TEntity>> ApplySorting { get; init; }
}