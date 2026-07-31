using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Business.Services.Categories;

public class CategoryService(TaskTrackerDbContext db) : ICategoryService
{

    public async Task<Result<CategoryBusinessModel>> GetByIdAsync(Guid id)
    {
        var response = await db.Categories
           .AsNoTracking()
           .Where(c => c.Id == id)
           .ProjectToType<CategoryBusinessModel>()
           .FirstOrDefaultAsync();

        if (response is null)
            return Result.Fail(new NotFoundError("Category", id));

        return Result.Ok(response);
    }

    public async Task<IReadOnlyCollection<CategoryBusinessModel>> GetAllAsync()
    {
        var result = await db.Categories
            .AsNoTracking()
            .ProjectToType<CategoryBusinessModel>()
            .ToListAsync();

        return result;
    }

    public async Task<Result<CategoryBusinessModel>> CreateAsync(CategorySaveCommand dto, Guid userId)
    {
        bool categoryExists = await db.Categories
            .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower());

        if (categoryExists)
            return Result.Fail(new ValidationError($"Category with name '{dto.Title}' already exists."));

        var entity = dto.Adapt<CategoryEntity>();
        entity.UserId = userId;

        db.Categories.Add(entity);
        await db.SaveChangesAsync();

        return Result.Ok(entity.Adapt<CategoryBusinessModel>());
    }


    public async Task<Result> UpdateAsync(CategorySaveCommand dto)
    {
        var category = await db.Categories.FindAsync(dto.Id);

        if (category == null)
            return Result.Fail(new NotFoundError("Category", dto.Id));

        if (db.Categories.Any(c => c.Title.ToLower() == dto.Title.ToLower() && c.Id != dto.Id))
            return Result.Fail(new ValidationError($"Category with name '{dto.Title}' already exists."));

        dto.Adapt(category);

        await db.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid categoryId)
    {
        int affectedRows = await db.Categories
            .Where(c => c.Id == categoryId)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
            return Result.Fail(new NotFoundError("Category", categoryId));

        return Result.Ok();
    }
}
