using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.Business.Extensions;

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
                ? dbQuery.OrderByDescending(c => c.Tasks.Count)
                : dbQuery.OrderBy(c => c.Tasks.Count),

            CategorySortField.CompletedTaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Tasks.Count(t => t.IsCompleted))
                : dbQuery.OrderBy(c => c.Tasks.Count(t => t.IsCompleted)),

            CategorySortField.NotCompletedTaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Tasks.Count(t => !t.IsCompleted))
                : dbQuery.OrderBy(c => c.Tasks.Count(t => !t.IsCompleted)),

            CategorySortField.Position => query.IsDescending
                ? dbQuery.OrderByDescending(c => c.Position)
                : dbQuery.OrderBy(c => c.Position),

            _ => dbQuery
        };
    }
}