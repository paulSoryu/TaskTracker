using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Business.Services.Reordering.Strategies;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering.Factories;

public class ReorderingStrategyFactory<TEntity>(
    CustomOrderReorderingStrategy<TEntity> customStrategy,
    SortedListReorderingStrategy<TEntity> sortedStrategy) 
    : IReorderingStrategyFactory<TEntity> 
    where TEntity : class, IOrderable
{
    public IReorderingStrategy<TEntity> GetStrategy(bool isCustomOrder)
        => isCustomOrder ? customStrategy : sortedStrategy;
}