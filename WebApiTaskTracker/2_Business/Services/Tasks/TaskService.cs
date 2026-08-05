using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
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
    public async Task<PagedResult<TaskView>> GetAllAsync(FilterTasksQuery filterQuery, SortTasksQuery sortQuery, PaginateTasksQuery paginateQuery)
    {
        // Ideally, we should pass the current date from the frontend, but for now, we will use the server's current date
        var today = DateOnly.FromDateTime(DateTime.Today);

        var tasksCountAfterFiltering = await db.Tasks
            .AsNoTracking()
            .ApplyFilter(filterQuery, today)
            .CountAsync();

        var result = await db.Tasks
            .AsNoTracking()
            .ApplyFilter(filterQuery, today)
            .ApplySorting(sortQuery)
            .ApplyPagination(paginateQuery)
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

    public async Task<Result<TaskView>> CreateAsync(SaveTaskCommand command, Guid userId)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == command.CategoryId);
        if (command.CategoryId != null && !categoryExists)
            return Result.Fail(new NotFoundError("Category", command.CategoryId));

        var pageExists = await db.Tasks
            .Skip((command.PageNumber - 1) * command.PageSize)
            .Take(command.PageSize)
            .AnyAsync();
        if (!pageExists)
            return Result.Fail(new NotFoundError("Page", command.PageNumber));

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
        int offset = (command.PageNumber - 1) * command.PageSize;
        int targetPosition = offset + 1;

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.Tasks
             .Where(t => t.Position >= targetPosition)
             .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position + 1));

            // Create the new task entity and set its position
            var entity = command.Adapt<TaskEntity>();
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

    public async Task<Result> UpdateAsync(SaveTaskCommand command)
    {
        var task = await db.Tasks.FindAsync(command.Id);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", command.Id!));

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == command.CategoryId);
        if (command.CategoryId != null && !categoryExists)
            return Result.Fail(new NotFoundError("Category", command.CategoryId));

        // Validate that the due date is not set to a past date, but only if the due date is being changed
        if (command.DueDate != task.DueDate && command.DueDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Fail(new ValidationError("You cannot change the due date to a past date."));

        command.Adapt(task);

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

    public async Task<Result> ReorderTaskAsync(MoveTaskCommand command)
    {
        var task = await db.Tasks
           .Select(t => new { t.Id, t.Position })
           .FirstOrDefaultAsync(t => t.Id == command.TaskId);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", command.TaskId));

        int offset = (command.PageNumber - 1) * command.PageSize;

        int totalTasks = await db.Tasks.CountAsync();
        if (offset >= totalTasks && totalTasks > 0)
            return Result.Fail(new NotFoundError("Page", command.PageNumber));

        int oldPos = task.Position;
        int newPos = Math.Clamp(command.NewLocalIndex + offset, 1, totalTasks);

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
                .Where(t => t.Id == command.TaskId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, newPos));

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Fail(new ReorderingError("Task", command.TaskId, oldPos, newPos, ex.Message));
        }
    }

    public async Task<Result> ResetAllPositionsAsync(MoveTaskCommand command, SortTasksQuery query)
    {
        var allTasks = await db.Tasks
            .ApplySorting(query)
            .ToListAsync();

        var movedTask = allTasks.FirstOrDefault(t => t.Id == command.TaskId);
        if (movedTask == null)
            return Result.Fail(new NotFoundError("Task", command.TaskId));

        int offset = (command.PageNumber - 1) * command.PageSize;
        if (offset >= allTasks.Count && allTasks.Count > 0)
            return Result.Fail(new NotFoundError("Page", command.PageNumber));

        // Save the old and new positions for logging purposes
        int loggedOldPos = movedTask.Position;
        int loggedNewPos = offset + command.NewLocalIndex;

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            allTasks.Remove(movedTask);

            int targetIndex = Math.Clamp(offset + command.NewLocalIndex - 1, 0, allTasks.Count);

            allTasks.Insert(targetIndex, movedTask);

            for (int i = 0; i < allTasks.Count; i++)
                allTasks[i].Position = i + 1;

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Fail(new ReorderingError("Task", command.TaskId, loggedOldPos, loggedNewPos, ex.Message));
        }
    }
}