using TaskTracker.Business.Services.Reordering.Strategies;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering.Factories;

public interface IReorderingStrategyFactory<TEntity> where TEntity : class, IOrderable
{
    IReorderingStrategy<TEntity> GetStrategy(bool isCustomOrder);
}