using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Business.Extensions;
using TaskTracker.Business.FluentErrors;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Enums;
using TaskTracker.Business.Models.Tasks;
using TaskTracker.DataAccess.Databases;
using TaskTracker.DataAccess.Entities;
using TaskTracker.Shared.Enums;
using TaskTracker.Shared.Constants;

namespace TaskTracker.Business.Services.Tasks;

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

    public async Task<Result<int>> GetPageById(Guid id, SortTasksQuery sortQuery, int pageSize)
    {
        var tasks = await db.Tasks
            .AsNoTracking()
            .ApplySorting(sortQuery)
            .ToListAsync();

        var task = tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", id));

        int taskIndex = tasks.IndexOf(task);
        int result = taskIndex / pageSize + 1; // +1 is because pages are 1-based

        return Result.Ok(result);
    }

    public async Task<Result<TaskView>> CreateAsync(SaveTaskCommand command, SortTasksQuery sortQuery, Guid userId)
    {
        // Check if the specified category exists in the database, but accept null values for the category ID, as tasks can be created without a category
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == command.CategoryId);
        if (command.CategoryId != null && !categoryExists)
            return Result.Fail(new NotFoundError("Category", command.CategoryId));

        // Check if the user has reached the maximum allowed tasks based on their email confirmation status
        bool isEmailConfirmed = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmailConfirmed)
            .FirstOrDefaultAsync();

        int currentTasksCount = await db.Tasks.CountAsync();
        int maxAllowedTasks = isEmailConfirmed
            ? TaskConstraints.MaxTasksForConfirmedEmail
            : TaskConstraints.MaxTasksForUnconfirmedEmail;

        if (currentTasksCount >= maxAllowedTasks)
            return Result.Fail(new TaskLimitExceededError(maxAllowedTasks, isEmailConfirmed));

        // Check if targeted task exists only in case FirstVisibleTaskIdOnPage isn't null
        var targetTask = await db.Tasks.FirstOrDefaultAsync(t => t.Id == command.FirstVisibleTaskIdOnPage);
        if (targetTask == null && command.FirstVisibleTaskIdOnPage != null)
            return Result.Fail(new NotFoundError("Task", command.FirstVisibleTaskIdOnPage));

        // Create new task in memory
        var createdTask = command.Adapt<TaskEntity>();
        createdTask.UserId = userId;

        int newPos = 1;
        // If any type of sorting besides "Custom Order" is enabled, insert a new task into a list and then reset the order of posistion in the whole list
        if (sortQuery.SortBy != TaskSortField.Position)
        {
            var allTasks = await db.Tasks
                .ApplySorting(sortQuery)
                .ToListAsync();

            if (command.FirstVisibleTaskIdOnPage != null)
                newPos = allTasks.IndexOf(targetTask!);

            allTasks.Insert(newPos, createdTask);
            db.Tasks.Add(createdTask);

            await db.ResetOrderAsync(allTasks, sortQuery.IsDescending);

            return Result.Ok(createdTask.Adapt<TaskView>());
        }

        // If "Custom Order" is enabled, reorder affected tasks and then insert a new task
        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            if (command.FirstVisibleTaskIdOnPage != null)
            {
                newPos = targetTask!.Position;
                if (sortQuery.IsDescending)
                    newPos += 1;

                createdTask.Position = newPos;
            }

            await db.ReorderInRangeAsync<TaskEntity>(currentTasksCount + 1, newPos);

            db.Tasks.Add(createdTask);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return Result.Ok(createdTask.Adapt<TaskView>());
        }
        catch
        {
            return Result.Fail(new ReorderingError("Task", newPos));
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

            var tasksCount = await db.Tasks.CountAsync();

            // Change the positions of tasks that were below the deleted task
            await db.ReorderInRangeAsync<TaskEntity>(deletedPos, tasksCount);

            await transaction.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ReorderingError("Task", taskId, deletedPos, ex.Message));
        }
    }

    public async Task<Result> MoveAsync(MoveTaskCommand command, SortTasksQuery sortQuery)
    {
        var task = await db.Tasks
           .FirstOrDefaultAsync(t => t.Id == command.TaskId);
        if (task == null)
            return Result.Fail(new NotFoundError("Task", command.TaskId));

        var targetTask = await db.Tasks
           .FirstOrDefaultAsync(t => t.Id == command.TargetTaskId);
        if (targetTask == null)
            return Result.Fail(new NotFoundError("Task", command.TargetTaskId));

        int oldPos = task.Position;
        int newPos = targetTask.Position;
        if (oldPos == newPos)
            return Result.Ok();

        // If any type of sorting besides "Custom Order" is enabled, insert a moved task into a list and then reset the order of posistion in the whole list
        if (sortQuery.SortBy != TaskSortField.Position)
        {
            var allTasks = await db.Tasks
                .ApplySorting(sortQuery)
                .ToListAsync();

            newPos = allTasks.IndexOf(targetTask);

            allTasks.Remove(task);
            allTasks.Insert(newPos, task); // -1 is because lists are 0-based

            await db.ResetOrderAsync(allTasks, sortQuery.IsDescending);

            return Result.Ok();
        }
        // If "Custom Order" is enabled, reorder affected tasks and then insert a moved task
        try
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            await db.ReorderInRangeAsync<TaskEntity>(oldPos, newPos);

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

    public async Task<Result> CreateDefaultTasksAsync(Guid userId, Dictionary<string, Guid> categoryIdsByName)
    {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var templates = new[]
        {
            (Title: "Prepare quarterly report",  Desc: "Pull Q2 numbers from the finance sheet, draft summary slides, send to review before Friday.",   Priority: TaskPriority.High,   DueDate: currentDate.AddDays(4),    Category: "Work"),
            (Title: "Book dentist appointment",  Desc: "Call the clinic on Karl Marx ave, ask for a morning slot next week.",                           Priority: TaskPriority.Medium, DueDate: currentDate.AddMonths(1),  Category: "Personal"),
            (Title: "Renew apartment insurance", Desc: "Compare two offers, pick the cheaper one with the same coverage, pay online.",                  Priority: TaskPriority.Low,    DueDate: currentDate.AddDays(15),   Category: "Errands"),
            (Title: "Grocery run",               Desc: "Milk, eggs, bread, coffee, something for Sunday dinner.",                                       Priority: TaskPriority.Low,    DueDate: (DateOnly?)null,           Category: "Health"),
            (Title: "Buy presents for kids",     Desc: "Check 3 options, compare prices, and purchase the best gifts.",                                 Priority: TaskPriority.High,   DueDate: currentDate.AddDays(-10),  Category: "Other")
        };

        var defaultTasks = templates.Select((t, index) => new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = t.Title,
            Description = t.Desc,
            Priority = t.Priority,
            DueDate = t.DueDate,
            UserId = userId,
            CategoryId = categoryIdsByName.GetValueOrDefault(t.Category, Guid.Empty),
            Position = templates.Length - index // Reversed order
        }).ToList();

        db.Tasks.AddRange(defaultTasks);

        var numOfChangedEntries = await db.SaveChangesAsync();

        return numOfChangedEntries > 0
            ? Result.Ok()
            : Result.Fail(new CreatingDefaultDataError("Task"));
    }

    public async Task DeleteAllByUserIdAsync(Guid userId)
    {
        await db.Tasks
            .IgnoreQueryFilters()
            .Where(t => t.UserId == userId)
            .ExecuteDeleteAsync();
    }
}