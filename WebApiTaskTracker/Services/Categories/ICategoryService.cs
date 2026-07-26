using WebApiTaskTracker.DTOs.Categories;

namespace WebApiTaskTracker.Services.Categories;

public interface ICategoryService
{
    Task<CategoryResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<CategorySummaryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CategoryCreateRequest category, Guid userId);
    Task UpdateAsync(Guid id, CategoryUpdateRequest category);

    Task DeleteAsync(Guid id);
}
