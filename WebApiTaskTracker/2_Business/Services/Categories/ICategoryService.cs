using FluentResults;
using WebApiTaskTracker.Business.Models.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public interface ICategoryService
{
    Task<Result<CategoryBusinessModel>> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<CategoryBusinessModel>> GetAllAsync();
    Task<Result<CategoryBusinessModel>> CreateAsync(CategorySaveCommand category, Guid userId);
    Task<Result> UpdateAsync(CategorySaveCommand category);
    Task<Result> DeleteAsync(Guid id);
}
