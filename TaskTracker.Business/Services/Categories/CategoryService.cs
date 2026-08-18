using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Business.Extensions;
using TaskTracker.Business.FluentErrors;
using TaskTracker.Business.Models.Categories;
using TaskTracker.Business.Models.Enums;
using TaskTracker.DataAccess.Databases;
using TaskTracker.DataAccess.Entities;
using TaskTracker.Shared.Constants;

namespace TaskTracker.Business.Services.Categories;

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

    public async Task<Result<CategoryView>> CreateAsync(SaveCategoryCommand command, SortCategoriesQuery query, Guid userId)
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

        if (query.SortBy == CategorySortField.Position)
        {
            db.Categories.Add(entity);
            await db.SaveChangesAsync();
        }
        else // if any kind of system sorting is active
        {
            var allCategories = await db.Categories
                .ApplySorting(query)
                .ToListAsync();

            allCategories.Add(entity);
            await db.ResetOrderAsync(allCategories, query.IsDescending);
        }
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
            return Result.Fail(new ReorderingError("Category", categoryId, deletedPos, ex.Message));
        }
    }

    public async Task<Result> MoveAsync(MoveCategoryCommand command, SortCategoriesQuery sortQuery)
    {
        var category = await db.Categories
           .FirstOrDefaultAsync(c => c.Id == command.CategoryId);
        if (category == null)
            return Result.Fail(new NotFoundError("Category", command.CategoryId));

        var targetCategory = await db.Categories
           .FirstOrDefaultAsync(c => c.Id == command.TargetCategoryId);
        if (targetCategory == null)
            return Result.Fail(new NotFoundError("Category", command.TargetCategoryId));

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
            return Result.Fail(new ReorderingError("Category", command.CategoryId, oldPos, newPos, ex.Message));
        }
    }

    public async Task<Result> DeleteTasksByCategoryIdAsync(Guid categoryId, bool deleteCompleted, bool deleteNotCompleted)
    {
        var category = await db.Categories
           .FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null)
            return Result.Fail(new NotFoundError("Category", categoryId));

        if (deleteCompleted && deleteNotCompleted)
            await db.Tasks
                .Where(t => t.CategoryId == categoryId)
                .ExecuteDeleteAsync();
        else if (deleteCompleted)
            await db.Tasks
                .Where(t => t.CategoryId == categoryId && t.IsCompleted)
                .ExecuteDeleteAsync();
        else
            await db.Tasks
                .Where(t => t.CategoryId == categoryId && !t.IsCompleted)
                .ExecuteDeleteAsync();

        return Result.Ok();
    }

    public async Task<Result<Dictionary<string, Guid>>> CreateDefaultCategoriesAsync(Guid userId)
    {
        var templates = new[]
        {
            (Title: "Work", Colour: "#B5602D"),
            (Title: "Personal", Colour: "#9C4A5C"),
            (Title: "Errands", Colour: "#5B6E9C"),
            (Title: "Health", Colour: "#2F6F6B"),
            (Title: "Other", Colour: "#8A8577")
        };

        var defaultCategories = templates.Select((t, index) => new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Title = t.Title,
            Colour = t.Colour,
            UserId = userId,
            Position = index + 1
        }).ToList();

        db.Categories.AddRange(defaultCategories);

        var numOfChangedEntries = await db.SaveChangesAsync();

        return numOfChangedEntries > 0
            ? Result.Ok(defaultCategories.ToDictionary(c => c.Title, c => c.Id))
            : Result.Fail<Dictionary<string, Guid>>(new CreatingDefaultDataError("Category"));
    }

    public async Task DeleteAllByUserIdAsync(Guid userId)
    {
        await db.Categories
            .IgnoreQueryFilters()
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync();
    }
}
