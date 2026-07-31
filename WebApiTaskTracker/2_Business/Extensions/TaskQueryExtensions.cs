using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Business.Extensions;

public static class TaskQueryExtensions
{
    public static IQueryable<TaskEntity> ApplyFilter(this IQueryable<TaskEntity> dbQuery, GetTasksQuery query, DateOnly today)
    {
        if (query.IsCompleted.HasValue)
            dbQuery = dbQuery.Where(t => t.IsCompleted == query.IsCompleted.Value);

        if (query.CategoryTitle != null)
            dbQuery = dbQuery.Where(t => t.Category != null && t.Category.Title == query.CategoryTitle);

        if (query.Priority.HasValue)
            dbQuery = dbQuery.Where(t => t.Priority == query.Priority.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.ToLower();
            dbQuery = dbQuery.Where(t => t.Title.ToLower().Contains(search)
                                    || (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        // Apply specific month filter if provided
        if (query.SpecificMonth.HasValue)
        {
            var startOfMonth = new DateOnly(query.SpecificMonth.Value.Year, query.SpecificMonth.Value.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            dbQuery = dbQuery.Where(t => t.DueDate >= startOfMonth && t.DueDate <= endOfMonth);
            return dbQuery;
        }

        // Apply due date filter preset if provided
        if (query.DueDateFilterPreset.HasValue)
        {
            return query.DueDateFilterPreset.Value switch
            {
                TaskDueDateFilterPreset.All => dbQuery,

                TaskDueDateFilterPreset.Overdue => dbQuery.Where(t => t.DueDate < today),

                TaskDueDateFilterPreset.Today => dbQuery.Where(t => t.DueDate == today),

                TaskDueDateFilterPreset.ThisWeek => dbQuery.Where(t => t.DueDate >= today.StartOfWeek() && t.DueDate <= today.EndOfWeek()),

                TaskDueDateFilterPreset.ThisMonth => dbQuery.Where(t => t.DueDate.Value.Year == today.Year && t.DueDate.Value.Month == today.Month),

                TaskDueDateFilterPreset.NoDueDate => dbQuery.Where(t => t.DueDate == null),

                _ => dbQuery
            };
        }

        return dbQuery;
    }

    public static IQueryable<TaskEntity> ApplySorting(this IQueryable<TaskEntity> dbQuery, GetTasksQuery query)
    {
        if (!query.SortBy.HasValue)
        {
            return dbQuery.OrderByDescending(t => t.CreatedAt);
        }

        return query.SortBy.Value switch
        {
            TaskSortField.Title => query.IsDescending
                ? dbQuery.OrderByDescending(t => t.Title)
                : dbQuery.OrderBy(t => t.Title),

            TaskSortField.CategoryTitle => query.IsDescending
                ? dbQuery.OrderByDescending(t => t.Category != null ? t.Category.Title : string.Empty)
                : dbQuery.OrderBy(t => t.Category != null ? t.Category.Title : string.Empty),

            TaskSortField.DueDate => query.IsDescending
                ? dbQuery.OrderByDescending(t => t.DueDate)
                : dbQuery.OrderBy(t => t.DueDate),

            TaskSortField.Priority => query.IsDescending
                ? dbQuery.OrderByDescending(t => t.Priority)
                : dbQuery.OrderBy(t => t.Priority),

            TaskSortField.IsCompleted => query.IsDescending
                ? dbQuery.OrderByDescending(t => t.IsCompleted)
                : dbQuery.OrderBy(t => t.IsCompleted),

            TaskSortField.CreatedAt => query.IsDescending
                ? dbQuery.OrderByDescending(t => t.CreatedAt)
                : dbQuery.OrderBy(t => t.CreatedAt),

            _ => dbQuery
        };
    }

    public static IQueryable<TaskEntity> ApplyPagination(this IQueryable<TaskEntity> dbQuery, GetTasksQuery query)
    {
        // default to page 1 and size 10 if not provided or invalid
        int page = query.PageNumber > 0 ? query.PageNumber : 1;
        int size = query.PageSize > 0 ? query.PageSize : 10;

        return dbQuery
            .Skip((page - 1) * size)
            .Take(size);
    }
}