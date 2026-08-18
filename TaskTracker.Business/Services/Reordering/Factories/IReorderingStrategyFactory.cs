using TaskTracker.Business.Services.Reordering.Strategies;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering.Factories;

public interface IReorderingStrategyFactory
{
    IReorderingStrategy<TEntity> GetStrategy<TEntity>(bool isCustomOrder) where TEntity : class, IOrderable;
}