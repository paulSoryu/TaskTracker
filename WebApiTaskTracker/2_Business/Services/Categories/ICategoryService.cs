using FluentResults;
using WebApiTaskTracker.Business.Models.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryView>> GetAllAsync(GetCategoriesQuery query);
    Task<Result<CategoryView>> GetByIdAsync(Guid id);
    Task<Result<CategoryView>> CreateAsync(SaveCategoryCommand category, Guid userId);
    Task<Result> UpdateAsync(SaveCategoryCommand category);
    Task<Result> DeleteAsync(Guid id);
}
