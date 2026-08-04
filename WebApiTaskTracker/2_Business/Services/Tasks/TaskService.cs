using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiTaskTracker.Business.Extensions;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Models;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Business.Services.Tasks;

public class TaskService(TaskTrackerDbContext db) : ITaskService
{
    public async Task<PagedResult<TaskView>> GetAllAsync(GetTasksQuery query)
    {
        // Ideally, we should pass the current date from the frontend, but for now, we will use the server's current date
        var today = DateOnly.FromDateTime(DateTime.Today);

        var tasksCountAfterFiltering = await db.Tasks
            .AsNoTracking()
            .ApplyFilter(query, today)
            .CountAsync();

        var result = await db.Tasks
            .AsNoTracking()
            .ApplyFilter(query, today)
            .ApplySorting(query)
            .ApplyPagination(query)
            .ProjectToType<TaskView>()
            .ToListAsync();

        return new PagedResult<TaskView>(result, tasksCountAfterFiltering);
    }

    public async Task<Result<TaskView>> GetByIdAsync(Guid id)
    {
        var response = await db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .ProjectToType<TaskView>()
            .FirstOrDefaultAsync();

        return response is null
            ? Result.Fail(new NotFoundError("Task", id))
            : Result.Ok(response);
    }

    public async Task<Result<TaskView>> CreateAsync(SaveTaskCommand dto, Guid userId)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (dto.CategoryId != null && !categoryExists)
            return Result.Fail(new NotFoundError("Category", dto.CategoryId));

        var pageExists = await db.Tasks
            .Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .AnyAsync();
        if (!pageExists)
            return Result.Fail(new NotFoundError("Page", dto.PageNumber));

        var isEmailConfirmed = await db.Users
            .Select(u => u.EmailConfirmed)
            .FirstOrDefaultAsync();

        int currentTasksCount = await db.Tasks.CountAsync();
        int maxAllowedTasks = isEmailConfirmed
            ? TaskConstraints.MaxTasksForConfirmedEmail
            : TaskConstraints.MaxTasksForUnconfirmedEmail;

        // Check if the user has reached the maximum allowed tasks
        if (currentTasksCount >= maxAllowedTasks)
            return Result.Fail(new TaskLimitExceededError(maxAllowedTasks, isEmailConfirmed));

        // Position calculations
        int offset = (dto.PageNumber - 1) * dto.PageSize;
        int targetPosition = offset + 1;

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.Tasks
             .Where(t => t.Position >= targetPosition)
             .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position + 1));

            // Create the new task entity and set its position
            var entity = dto.Adapt<TaskEntity>();
            entity.Position = targetPosition;
            entity.UserId = userId;

            db.Tasks.Add(entity);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return Result.Ok(entity.Adapt<TaskView>());
        }
        catch
        {
            await transaction.RollbackAsync();
            return Result.Fail(new ReorderingError("Task", targetPosition));
        }
    }

    public async Task<Result> UpdateAsync(SaveTaskCommand dto)
    {
        var task = await db.Tasks.FindAsync(dto.Id);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", dto.Id!));

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (dto.CategoryId != null && !categoryExists)
            return Result.Fail(new NotFoundError("Category", dto.CategoryId));

        // Validate that the due date is not set to a past date, but only if the due date is being changed
        if (dto.DueDate != task.DueDate && dto.DueDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Fail(new ValidationError("You cannot change the due date to a past date."));

        dto.Adapt(task);

        await db.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid taskId)
    {
        var task = await db.Tasks
                .Select(t => new { t.Id, t.Position })
                .FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", taskId));

        int deletedPos = task.Position;

        try
        {
            await db.Database.BeginTransactionAsync();
            
            await db.Tasks
                .Where(t => t.Id == taskId)
                .ExecuteDeleteAsync();

            // Shift up the positions of tasks that were below the deleted task
            await db.Tasks
                .Where(t => t.Position > deletedPos)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position - 1));

            await db.Database.CommitTransactionAsync();
            return Result.Ok();
        }
        catch
        {
            await db.Database.RollbackTransactionAsync();
            return Result.Fail(new ReorderingError("Task", taskId, deletedPos));
        }
    }

    public async Task<Result> ReorderTaskAsync(MoveTaskCommand dto)
    {
        var task = await db.Tasks
                .Select(t => new { t.Id, t.Position })
                .FirstOrDefaultAsync(t => t.Id == dto.TaskId);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", dto.TaskId));

        var pageExists = await db.Tasks
            .Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .AnyAsync();
        if (!pageExists)
            return Result.Fail(new NotFoundError("Page", dto.PageNumber));

        int offset = (dto.PageNumber - 1) * dto.PageSize;

        int oldPos = task.Position;
        int newPos = offset + dto.NewLocalIndex;

        if (oldPos == newPos) return Result.Ok();

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            if (oldPos < newPos)
            {
                // Downshift
                await db.Tasks
                    .Where(t => t.Position > oldPos && t.Position <= newPos)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position - 1));
            }
            else
            {
                // Upshift
                await db.Tasks
                    .Where(t => t.Position >= newPos && t.Position < oldPos)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position + 1));
            }

            // Update the position of the moved task
            await db.Tasks
                .Where(t => t.Id == dto.TaskId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, newPos));

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch
        {
            await transaction.RollbackAsync();
            return Result.Fail(new ReorderingError("Task", dto.TaskId, oldPos, newPos));
        }
    }


}
