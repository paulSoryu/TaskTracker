using FluentResults;
using WebApiTaskTracker.Business.Models.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryBusinessModel>> GetAllAsync(GetCategoriesQuery query);
    Task<Result<CategoryBusinessModel>> GetByIdAsync(Guid id);
    Task<Result<CategoryBusinessModel>> CreateAsync(CategorySaveCommand category, Guid userId);
    Task<Result> UpdateAsync(CategorySaveCommand category);
    Task<Result> DeleteAsync(Guid id);
}
