namespace TaskTracker.Business.Models.Tasks;

public record PaginateTasksQuery(
    int PageNumber,
    int PageSize
);