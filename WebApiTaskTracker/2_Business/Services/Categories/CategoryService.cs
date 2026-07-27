using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public class CategoryService(TaskTrackerDbContext db) : ICategoryService
{

    public async Task<CategoryBusinessModel> GetByIdAsync(Guid id)
    {
        var response = await db.Categories
           .AsNoTracking()
           .Where(c => c.Id == id)
           .ProjectToType<CategoryBusinessModel>()
           .FirstOrDefaultAsync();

        return response ?? throw new EntityNotFoundException($"Category {id} not found.");
    }

    public async Task<IReadOnlyCollection<CategoryBusinessModel>> GetAllAsync()
    {
        var result = await db.Categories
            .AsNoTracking()
            .ProjectToType<CategoryBusinessModel>()
            .ToListAsync();

        return result;
    }

    public async Task<CategoryBusinessModel> CreateAsync(CategorySaveCommand dto, Guid userId)
    {
        bool categoryExists = await db.Categories
            .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower());

        if (categoryExists)
            throw new EntityAlreadyExistsException($"Category with name '{dto.Title}' already exists.");

        var entity = dto.Adapt<CategoryEntity>();
        entity.UserId = userId;

        db.Categories.Add(entity);
        await db.SaveChangesAsync();

        return entity.Adapt<CategoryBusinessModel>();
    }


    public async Task UpdateAsync(CategorySaveCommand dto)
    {
        var category = await db.Categories.FindAsync(dto.Id);

        if (category == null)
            throw new EntityNotFoundException($"Category {dto.Id} not found.");

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
