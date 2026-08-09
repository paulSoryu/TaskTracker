using FluentResults;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryView>> GetAllAsync(FilterCategoriesQuery filterQuery, SortCategoriesQuery sortQuery);
    Task<Result<CategoryView>> GetByIdAsync(Guid id);
    Task<Result<CategoryView>> CreateAsync(SaveCategoryCommand category, Guid userId);
    Task<Result> UpdateAsync(SaveCategoryCommand category);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> MoveAsync(MoveCategoryCommand command, SortCategoriesQuery sortQuery);
    Task<Result> DeleteTasksAsync(Guid id, bool deleteCompleted, bool deleteNotCompleted);
}
