namespace TaskTracker.Business.Models;

public record PagedResult<T>(
    IReadOnlyCollection<T> Items, 
    int TotalCount
);
