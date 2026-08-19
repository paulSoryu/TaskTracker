namespace TaskTracker.Business.Models.Users;

public record PaginateUsersQuery(
    int PageNumber,
    int PageSize
);
