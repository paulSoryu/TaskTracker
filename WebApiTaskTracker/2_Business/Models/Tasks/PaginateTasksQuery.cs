namespace WebApiTaskTracker.Business.Models.Tasks;

public record PaginateTasksQuery(
    int PageNumber,
    int PageSize
);