using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Extensions;

public static class CategoryQueryExtensions
{
    public static IQueryable<CategoryEntity> ApplyFilter(this IQueryable<CategoryEntity> dbQuery, GetCategoriesQuery query)
    {
        if (query.SearchTerm != null)
            dbQuery = dbQuery.Where(c => c.Title.Contains(query.SearchTerm));
        
        return dbQuery;
    }

    public static IQueryable<CategoryEntity> ApplySorting(this IQueryable<CategoryEntity> dbQuery, GetCategoriesQuery query)
    {
        if (!query.SortBy.HasValue)
        {
            return dbQuery;
        }

        return query.SortBy.Value switch
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

            _ => dbQuery
        };
    }
}