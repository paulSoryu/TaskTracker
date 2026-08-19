using TaskTracker.Business.Models.Enums;
using TaskTracker.Business.Models.Tasks;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Extensions;

public static class TaskQueryExtensions
{
    public static IQueryable<TaskEntity> ApplyFilter(this IQueryable<TaskEntity> dbQuery, FilterTasksQuery query)
    {
        if (query.IsCompleted.HasValue)
            dbQuery = dbQuery.Where(t => t.IsCompleted == query.IsCompleted.Value);
        
        if (query.FilterByNoCategory == true)
            dbQuery = dbQuery.Where(t => t.CategoryId == null);
        else if (query.CategoryId.HasValue)
            dbQuery = dbQuery.Where(t => t.CategoryId == query.CategoryId.Value);

        if (query.Priority.HasValue)
            dbQuery = dbQuery.Where(t => t.Priority == query.Priority.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm;

            dbQuery = dbQuery.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

        }

        // Apply specific month filter if provided
        if (query.SpecificMonth.HasValue)
        {
            var startOfMonth = new DateOnly(query.SpecificMonth.Value.Year, query.SpecificMonth.Value.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            dbQuery = dbQuery.Where(t => t.DueDate >= startOfMonth && t.DueDate <= endOfMonth);
        }
        // Apply due date filter preset if provided
        else if (query.DueDateFilterPreset.HasValue)
        {
            var today = query.ClientToday;
            var startOfWeek = today.StartOfWeek();
            var endOfWeek = today.EndOfWeek();
            var startOfMonth = new DateOnly(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            dbQuery = query.DueDateFilterPreset.Value switch
            {
                TaskDueDateFilterPreset.Overdue => dbQuery.Where(t => t.DueDate < today),

                TaskDueDateFilterPreset.Today => dbQuery.Where(t => t.DueDate == today),

                TaskDueDateFilterPreset.ThisWeek => dbQuery.Where(t => t.DueDate >= startOfWeek && t.DueDate <= endOfWeek),

                TaskDueDateFilterPreset.ThisMonth => dbQuery.Where(t => t.DueDate >= startOfMonth && t.DueDate <= endOfMonth),

                TaskDueDateFilterPreset.NoDueDate => dbQuery.Where(t => t.DueDate == null),

                _ => dbQuery
            };
        }

        return dbQuery;
    }

    public static IQueryable<TaskEntity> ApplySorting(this IQueryable<TaskEntity> dbQuery, SortTasksQuery query)
    {
        return query.SortBy switch
        {
            TaskSortField.Title => query.IsDescending == true
                ? dbQuery.OrderByDescending(t => t.Title)
                : dbQuery.OrderBy(t => t.Title),

            TaskSortField.CategoryTitle => query.IsDescending == true
                ? dbQuery.OrderByDescending(t => t.Category.Title)
                : dbQuery.OrderBy(t => t.Category.Title),

            TaskSortField.DueDate => query.IsDescending == true
                ? dbQuery.OrderByDescending(t => t.DueDate)
                : dbQuery.OrderBy(t => t.DueDate),

            TaskSortField.Priority => query.IsDescending == true
                ? dbQuery.OrderByDescending(t => t.Priority)
                : dbQuery.OrderBy(t => t.Priority),

            TaskSortField.IsCompleted => query.IsDescending == true
                ? dbQuery.OrderByDescending(t => t.IsCompleted)
                : dbQuery.OrderBy(t => t.IsCompleted),

            TaskSortField.CreatedAt => query.IsDescending == true
                ? dbQuery.OrderByDescending(t => t.CreatedAt)
                : dbQuery.OrderBy(t => t.CreatedAt),

            TaskSortField.Position => query.IsDescending == true
                ? dbQuery.OrderByDescending(t => t.Position)
                : dbQuery.OrderBy(t => t.Position),

            _ => dbQuery
        };
    }

    public static IQueryable<TaskEntity> ApplyPagination(this IQueryable<TaskEntity> dbQuery, PaginateTasksQuery query)
    {
        int page = query.PageNumber > 0 ? query.PageNumber : 1;
        int size = query.PageSize > 0 ? query.PageSize : 10;

        return dbQuery
            .Skip((page - 1) * size)
            .Take(size);
    }
}