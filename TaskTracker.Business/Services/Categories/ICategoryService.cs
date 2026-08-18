using FluentResults;
using TaskTracker.Business.Models.Categories;

namespace TaskTracker.Business.Services.Categories;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryView>> GetAllAsync(FilterCategoriesQuery filterQuery, SortCategoriesQuery sortQuery);
    Task<Result<CategoryView>> GetByIdAsync(Guid id);
    Task<Result<CategoryView>> CreateAsync(SaveCategoryCommand category, SortCategoriesQuery query, Guid userId);
    Task<Result> UpdateAsync(SaveCategoryCommand category);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> MoveAsync(MoveCategoryCommand command, SortCategoriesQuery sortQuery);
    Task<Result> DeleteTasksByCategoryIdAsync(Guid id, bool deleteCompleted, bool deleteNotCompleted);
    Task<Result<Dictionary<string, Guid>>> CreateDefaultCategoriesAsync(Guid userId);
    Task DeleteAllByUserIdAsync(Guid userId);
}
