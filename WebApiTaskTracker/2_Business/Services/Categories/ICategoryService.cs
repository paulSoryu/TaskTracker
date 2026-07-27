using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public interface ICategoryService
{
    Task<CategoryBusinessModel> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<CategoryBusinessModel>> GetAllAsync();
    Task<CategoryBusinessModel> CreateAsync(CategorySaveCommand category, Guid userId);
    Task UpdateAsync(CategorySaveCommand category);
    Task DeleteAsync(Guid id);
}
