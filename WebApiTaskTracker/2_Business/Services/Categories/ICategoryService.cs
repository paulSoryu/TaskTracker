using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public interface ICategoryService
{
    Task<CategoryResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<CategorySummaryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CategoryCreateRequest category, Guid userId);
    Task UpdateAsync(Guid id, CategoryUpdateRequest category);

    Task DeleteAsync(Guid id);
}
