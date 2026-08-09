using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiTaskTracker.Business.Extensions;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.Business.Services.Categories;

public class CategoryService(TaskTrackerDbContext db) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryView>> GetAllAsync(FilterCategoriesQuery filterQuery, SortCategoriesQuery sortQuery)
    {
        var result = await db.Categories
            .AsNoTracking()
            .ApplyFilter(filterQuery)
            .ApplySorting(sortQuery)
            .ProjectToType<CategoryView>()
            .ToListAsync();

        return result;
    }

    public async Task<Result<CategoryView>> GetByIdAsync(Guid id)
    {
        var response = await db.Categories
           .AsNoTracking()
           .Where(c => c.Id == id)
           .ProjectToType<CategoryView>()
           .FirstOrDefaultAsync();

        return response is null
            ? Result.Fail(new NotFoundError("Category", id))
            : Result.Ok(response);
    }

    public async Task<Result<CategoryView>> CreateAsync(SaveCategoryCommand command, Guid userId)
    {
        // Check if category with the same name already exists
        bool categoryExists = await db.Categories
            .AnyAsync(c => c.Title == command.Title);

        if (categoryExists)
            return Result.Fail(new ValidationError($"Category with name '{command.Title}' already exists."));

        // Check if the user has reached the maximum allowed categories
        var isEmailConfirmed = await db.Users
            .Select(u => u.EmailConfirmed)
            .FirstOrDefaultAsync();

        int currentCategoriesCount = await db.Categories.CountAsync();
        int maxAllowedCategories = isEmailConfirmed 
            ? CategoryConstraints.MaxCategoriesForConfirmedEmail 
            : CategoryConstraints.MaxCategoriesForUnconfirmedEmail;

        
        if (currentCategoriesCount >= maxAllowedCategories)
            return Result.Fail(new CategoryLimitExceededError(maxAllowedCategories, isEmailConfirmed));

        // Create category
        var entity = command.Adapt<CategoryEntity>();
        entity.UserId = userId;
        entity.Position = currentCategoriesCount + 1;

        db.Categories.Add(entity);
        await db.SaveChangesAsync();

        return Result.Ok(entity.Adapt<CategoryView>());
    }


    public async Task<Result> UpdateAsync(SaveCategoryCommand command)
    {
        var category = await db.Categories.FindAsync(command.Id);

        if (category == null)
            return Result.Fail(new NotFoundError("Category", command.Id));

        if (db.Categories.Any(c => c.Title.ToLower() == command.Title.ToLower() && c.Id != command.Id))
            return Result.Fail(new ValidationError($"Category with name '{command.Title}' already exists."));

        command.Adapt(category);

        await db.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid categoryId)
    {
        var categoriesCount = await db.Categories.CountAsync();
        if (categoriesCount <= 1)
            return Result.Fail(new ValidationError("Cannot delete the last category."));

        var category = await db.Categories
                .Select(c => new { c.Id, c.Position })
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null)
            return Result.Fail(new NotFoundError("Category", categoryId));

        int deletedPos = category.Position;

        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            await db.Categories
                .Where(c => c.Id == categoryId)
                .ExecuteDeleteAsync();

            var tasksCount = await db.Tasks.CountAsync();

            // Change the positions of tasks that were below the deleted task
            await db.ReorderInRangeAsync<CategoryEntity>(deletedPos, tasksCount);

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ReorderingError("Task", categoryId, deletedPos, ex.Message));
        }
    }

    public async Task<Result> MoveAsync(MoveCategoryCommand command, SortCategoriesQuery sortQuery)
    {
        var category = await db.Categories
           .FirstOrDefaultAsync(c => c.Id == command.CategoryId);
        if (category == null)
            return Result.Fail(new NotFoundError("Task", command.CategoryId));

        var targetCategory = await db.Categories
           .FirstOrDefaultAsync(c => c.Id == command.TargetCategoryId);
        if (targetCategory == null)
            return Result.Fail(new NotFoundError("Task", command.TargetCategoryId));

        int oldPos = category.Position;
        int newPos = targetCategory.Position;
        if (oldPos == newPos)
            return Result.Ok();

        // If any type of sorting besides "Custom Order" is enabled, insert a moved category into a list and then reset the order of posistion in the whole list
        if (sortQuery.SortBy != CategorySortField.Position)
        {
            var allCategories = await db.Categories
                .ApplySorting(sortQuery)
                .ToListAsync();

            newPos = allCategories.IndexOf(targetCategory);

            allCategories.Remove(category);
            allCategories.Insert(newPos, category); // -1 is because lists are 0-based

            await db.ResetOrderAsync(allCategories, sortQuery.IsDescending);

            return Result.Ok();
        }
        // If "Custom Order" is enabled, reorder affected categories and then insert a moved category
        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            await db.ReorderInRangeAsync<CategoryEntity>(oldPos, newPos);

            await db.Categories
                .Where(c => c.Id == command.CategoryId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Position, newPos));

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ReorderingError("Task", command.CategoryId, oldPos, newPos, ex.Message));
        }
    }
}
