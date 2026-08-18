using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Business.Services.Reordering.Strategies;
using TaskTracker.DataAccess.Interfaces;

namespace TaskTracker.Business.Services.Reordering.Factories;

public class ReorderingStrategyFactory(IServiceProvider serviceProvider) : IReorderingStrategyFactory
{
    public IReorderingStrategy<TEntity> GetStrategy<TEntity>(bool isCustomOrder) where TEntity : class, IOrderable
        => isCustomOrder
            ? serviceProvider.GetRequiredService<CustomOrderReorderingStrategy<TEntity>>()
            : serviceProvider.GetRequiredService<SortedListReorderingStrategy<TEntity>>();
}