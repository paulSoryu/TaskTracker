using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public class TaskService(TaskTrackerDbContext db) : ITaskService
{

    public async Task<TaskResponse> GetByIdAsync(Guid id)
    {
        var response = await db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .ProjectToType<TaskResponse>()
            .FirstOrDefaultAsync();

        return response ?? throw new EntityNotFoundException($"Task {id} not found.");
    }

    public async Task<IEnumerable<TaskSummaryResponse>> GetAllAsync()
    {
        return await db.Tasks
            .AsNoTracking()
            .ProjectToType<TaskSummaryResponse>()
            .ToListAsync();
    }

    public async Task<TaskResponse> CreateAsync(TaskCreateRequest dto, Guid userId)
    {
        var existingCategory = await GetOrCreateCategoryAsync(dto.CategoryTitle, userId);
        Guid? categoryId = existingCategory?.Id;

        var entity = dto.Adapt<TaskEntity>();

        entity.UserId = userId;
        entity.CategoryId = categoryId;

        db.Tasks.Add(entity);
        await db.SaveChangesAsync();

        return entity.Adapt<TaskResponse>();
    }

    public async Task UpdateAsync(Guid taskId, TaskUpdateRequest dto)
    {
        var task = await db.Tasks.FindAsync(taskId);
        if (task == null)
            throw new EntityNotFoundException($"Task {taskId} not found.");

        dto.Adapt(task);

        if (dto.CategoryName != null)
        {
            var category = await GetOrCreateCategoryAsync(dto.CategoryName, task.UserId);
            task.CategoryId = category?.Id;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid taskId)
    {
        var existingTask = await db.Tasks.FindAsync(taskId);
        if (existingTask == null)
            throw new EntityNotFoundException($"Task {taskId} not found.");

        db.Remove(existingTask);
        await db.SaveChangesAsync();
    }

    private async Task<CategoryEntity?> GetOrCreateCategoryAsync(string categoryName, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

        var cleanedTitle = categoryName.Trim();
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
