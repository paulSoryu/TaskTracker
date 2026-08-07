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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task<Result<TaskView>> CreateAsync(SaveTaskCommand command, bool isDescendingSorting, Guid userId)
    {
        // Check if the specified category exists in the database, but accept null values for the category ID, as tasks can be created without a category
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == command.CategoryId);
        if (command.CategoryId != null && !categoryExists)
            return Result.Fail(new NotFoundError("Category", command.CategoryId));

        // Check if the user has reached the maximum allowed tasks based on their email confirmation status
        var isEmailConfirmed = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmailConfirmed)
            .FirstOrDefaultAsync();

        int currentTasksCount = await db.Tasks.CountAsync();
        int maxAllowedTasks = isEmailConfirmed
            ? TaskConstraints.MaxTasksForConfirmedEmail
            : TaskConstraints.MaxTasksForUnconfirmedEmail;

        if (currentTasksCount >= maxAllowedTasks)
            return Result.Fail(new TaskLimitExceededError(maxAllowedTasks, isEmailConfirmed));

        // Check if the requested page number is valid based on the current number of tasks and the provided pagination parameters
        int offset = (command.PageNumber - 1) * command.PageSize;

        if (command.PageNumber > 1 && offset >= currentTasksCount)
            return Result.Fail(new NotFoundError("Page", command.PageNumber));

        // Position calculations
        int targetPosition = isDescendingSorting
            ? currentTasksCount - offset + 1
            : offset + 1;

        // Unfinished, better off refactoring this to a separate method that can be called after any operation that changes the order of tasks, like create, delete, or reorder.
        //if (sortBy != SortField.Position)
        //{
        //    var allTasks = await db.Tasks
        //    .ApplySorting(query)
        //    .ToListAsync();

        //    for (int i = 0; i < allTasks.Count; i++)
        //        allTasks[i].Position = i + 1;

        //    return Result.Ok();
        //}

        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            await db.Tasks
             .Where(t => t.Position >= targetPosition)
             .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position + 1));

            // Create new task entity and set its position and owner
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
            using var transaction = await db.Database.BeginTransactionAsync();

            await db.Tasks
                .Where(t => t.Id == taskId)
                .ExecuteDeleteAsync();

            // Shift up the positions of tasks that were below the deleted task
            await db.Tasks
                .Where(t => t.Position > deletedPos)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Position, t => t.Position - 1));

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ReorderingError("Task", taskId, deletedPos, ex.Message));
        }
    }

    public async Task<Result> ReorderTaskAsync(MoveTaskCommand command, bool isDescendingSorting)
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

        int newLocalIndex = isDescendingSorting
            ? totalTasks - (command.NewLocalIndex + offset) + 1
            : command.NewLocalIndex + offset;

        int oldPos = task.Position;
        int newPos = Math.Clamp(newLocalIndex, 1, totalTasks);

        if (oldPos == newPos) 
            return Result.Ok();

        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

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

        allTasks.Remove(movedTask);

        int newLocalIndex = query.IsDescending
            ? allTasks.Count - (command.NewLocalIndex + offset) + 1
            : command.NewLocalIndex + offset;

        int targetIndex = Math.Clamp(newLocalIndex - 1, 0, allTasks.Count); // -1 is because lists are 0-based
        allTasks.Insert(targetIndex, movedTask);

        for (int i = 0; i < allTasks.Count; i++)
            allTasks[i].Position = i + 1;

        await db.SaveChangesAsync();

        return Result.Ok();
    }
}