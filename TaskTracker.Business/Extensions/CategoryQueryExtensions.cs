using TaskTracker.Business.Models.Categories;
using TaskTracker.Business.Models.Enums;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Extensions;

public static class CategoryQueryExtensions
{
    public static IQueryable<CategoryEntity> ApplyFilter(this IQueryable<CategoryEntity> dbQuery, FilterCategoriesQuery query)
    {
        if (query.SearchTerm != null)
            dbQuery = dbQuery.Where(c => c.Title.Contains(query.SearchTerm));
        
        return dbQuery;
    }

    public static IQueryable<CategoryEntity> ApplySorting(this IQueryable<CategoryEntity> dbQuery, SortCategoriesQuery query)
    {
        return query.SortBy switch
        {
            CategorySortField.Title => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Title)
                : dbQuery.OrderBy(c => c.Title),

            CategorySortField.TaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Tasks.Count).ThenBy(c => c.Title)
                : dbQuery.OrderBy(c => c.Tasks.Count).ThenBy(c => c.Title),

            CategorySortField.CompletedTaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Tasks.Count(t => t.IsCompleted)).ThenBy(c => c.Title)
                : dbQuery.OrderBy(c => c.Tasks.Count(t => t.IsCompleted)).ThenBy(c => c.Title),

            CategorySortField.NotCompletedTaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Tasks.Count(t => !t.IsCompleted)).ThenBy(c => c.Title)
                : dbQuery.OrderBy(c => c.Tasks.Count(t => !t.IsCompleted)).ThenBy(c => c.Title),

            CategorySortField.Position => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Position)
                : dbQuery.OrderBy(c => c.Position),

            _ => dbQuery
        };
    }
}