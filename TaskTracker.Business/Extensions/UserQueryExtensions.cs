using TaskTracker.Business.Models.Enums;
using TaskTracker.Business.Models.Users;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Extensions;

public static class UserQueryExtensions
{
    public static IQueryable<UserEntity> ApplyFilter(this IQueryable<UserEntity> dbQuery, FilterUsersQuery query)
    {
        if (query.SearchTerm != null)
            dbQuery = dbQuery.Where(u => u.Email!.Contains(query.SearchTerm));
        return dbQuery;
    }

    public static IQueryable<UserEntity> ApplySorting(this IQueryable<UserEntity> dbQuery, SortUsersQuery query)
    {
        return query.SortBy switch
        {
            UserSortField.Email => query.IsDescending
                ? dbQuery.OrderByDescending(u => u.Email)
                : dbQuery.OrderBy(u => u.Email),

            UserSortField.CreatedAt => query.IsDescending
                ? dbQuery.OrderByDescending(u => u.CreatedAt)
                : dbQuery.OrderBy(u => u.CreatedAt),

            UserSortField.LastOnlineTime => query.IsDescending
                ? dbQuery.OrderByDescending(u => u.LastOnlineTime)
                : dbQuery.OrderBy(u => u.LastOnlineTime),

            UserSortField.TaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(u => u.Tasks.Count).ThenBy(u => u.Email)
                : dbQuery.OrderBy(u => u.Tasks.Count).ThenBy(u => u.Email),

            UserSortField.CompletedTaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(u => u.Tasks.Count(t => t.IsCompleted)).ThenBy(u => u.Email)
                : dbQuery.OrderBy(u => u.Tasks.Count(t => t.IsCompleted)).ThenBy(u => u.Email),

            UserSortField.NotCompletedTaskCount => query.IsDescending
                ? dbQuery.OrderByDescending(u => u.Tasks.Count(t => !t.IsCompleted)).ThenBy(u => u.Email)
                : dbQuery.OrderBy(u => u.Tasks.Count(t => !t.IsCompleted)).ThenBy(u => u.Email),

            UserSortField.CategoryCount => query.IsDescending
                ? dbQuery.OrderByDescending(u => u.Categories.Count).ThenBy(u => u.Email)
                : dbQuery.OrderBy(u => u.Categories.Count).ThenBy(u => u.Email),

            _ => dbQuery
        };
    }

    public static IQueryable<UserEntity> ApplyPagination(this IQueryable<UserEntity> dbQuery, PaginateUsersQuery query)
    {
        int page = query.PageNumber > 0 ? query.PageNumber : 1;
        int size = query.PageSize > 0 ? query.PageSize : 10;

        return dbQuery
            .Skip((page - 1) * size)
            .Take(size);
    }
}
