using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiTaskTracker.Data.Databases;
using WebApiTaskTracker.Data.Entities;
using WebApiTaskTracker.DTOs.Categories;
using WebApiTaskTracker.Services.Categories;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Services.Tasks;

public class CategoryService(TaskTrackerDbContext db) : ICategoryService
{

    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var response = await db.Categories
           .AsNoTracking()
           .Where(c => c.Id == id)
           .ProjectToType<CategoryResponse>()
           .FirstOrDefaultAsync();

        return response ?? throw new EntityNotFoundException($"Category {id} not found.");
    }

    public async Task<IEnumerable<CategorySummaryResponse>> GetAllAsync()
    {
        var result = await db.Categories
            .AsNoTracking()
            .ProjectToType<CategorySummaryResponse>()
            .ToListAsync();

        return result;
    }

    public async Task<CategoryResponse> CreateAsync(CategoryCreateRequest dto, Guid userId)
    {
        bool categoryExists = await db.Categories
            .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower());

        if (categoryExists)
            throw new EntityAlreadyExistsException($"Category with name '{dto.Title}' already exists.");

        var entity = dto.Adapt<CategoryEntity>();
        entity.UserId = userId;

        db.Categories.Add(entity);
        await db.SaveChangesAsync();

        return entity.Adapt<CategoryResponse>();
    }


    public async Task UpdateAsync(Guid categoryId, CategoryUpdateRequest dto)
    {
        var category = await db.Categories.FindAsync(categoryId);

        if (category == null)
            throw new EntityNotFoundException($"Category {categoryId} not found.");

        dto.Adapt(category);

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid categoryId)
    {
        int affectedRows = await db.Categories
            .Where(c => c.Id == categoryId)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
            throw new EntityNotFoundException($"Category {categoryId} not found.");
    }
}
