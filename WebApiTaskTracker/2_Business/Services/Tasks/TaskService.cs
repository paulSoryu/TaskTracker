using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiTaskTracker.Business.Extensions;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Services.Tasks;

public class TaskService(TaskTrackerDbContext db) : ITaskService
{
    public async Task<IReadOnlyCollection<TaskBusinessModel>> GetAllAsync(GetTasksQuery query)
    {
        // Ideally, we should pass the current date from the frontend, but for now, we will use the server's current date
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await db.Tasks.AsQueryable()
            .AsNoTracking()
            .ApplyFilter(query, today)
            .ApplySorting(query)
            .ApplyPagination(query)
            .ProjectToType<TaskBusinessModel>()
            .ToListAsync();
    }

    public async Task<Result<TaskBusinessModel>> GetByIdAsync(Guid id)
    {
        var response = await db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .ProjectToType<TaskBusinessModel>()
            .FirstOrDefaultAsync();

        return response is null
            ? Result.Fail(new NotFoundError("Task", id))
            : Result.Ok(response);
    }

    public async Task<Result<TaskBusinessModel>> CreateAsync(TaskSaveCommand dto, Guid userId)
    {
        // Global SQL filter already filters everything by userId, so we don't need to filter here
        var isEmailConfirmed = await db.Users
            .Select(u => u.EmailConfirmed)
            .FirstOrDefaultAsync();

        int currentTasksCount = await db.Tasks.CountAsync();
        int maxAllowedTasks = isEmailConfirmed ? 1000 : 20;

        // Check if the user has reached the maximum allowed tasks
        if (currentTasksCount >= maxAllowedTasks)
            return Result.Fail(new TaskLimitExceededError(maxAllowedTasks, isEmailConfirmed));

        var existingCategory = await GetOrCreateCategoryAsync(dto.CategoryTitle!, userId);
        Guid? categoryId = existingCategory?.Id;

        var entity = dto.Adapt<TaskEntity>();

        entity.UserId = userId;
        entity.CategoryId = categoryId;

        db.Tasks.Add(entity);
        await db.SaveChangesAsync();

        return Result.Ok(entity.Adapt<TaskBusinessModel>());
    }

    public async Task<Result> UpdateAsync(TaskSaveCommand dto)
    {
        var task = await db.Tasks.FindAsync(dto.Id);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", dto.Id));

        // Validate that the due date is not set to a past date, but only if the due date is being changed
        if (dto.DueDate != task.DueDate && dto.DueDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Fail(new ValidationError("You cannot change the due date to a past date."));

        dto.Adapt(task);

        if (dto.CategoryTitle != null)
        {
            var category = await GetOrCreateCategoryAsync(dto.CategoryTitle, task.UserId);
            task.CategoryId = category?.Id;
        }

        await db.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid taskId)
    {
        var existingTask = await db.Tasks.FindAsync(taskId);
        if (existingTask == null)
            return Result.Fail(new NotFoundError("Task", taskId));

        db.Remove(existingTask);
        await db.SaveChangesAsync();

        return Result.Ok();
    }

    private async Task<CategoryEntity?> GetOrCreateCategoryAsync(string? categoryTitle, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(categoryTitle))
            return null;

        var cleanedTitle = categoryTitle.Trim();
        var normalizedTitle = cleanedTitle.ToLower();

        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Title.ToLower() == normalizedTitle);

        if (category == null)
        {
            category = new CategoryEntity
            {
                Title = cleanedTitle,
                UserId = userId
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();
        }

        return category;
    }
}
