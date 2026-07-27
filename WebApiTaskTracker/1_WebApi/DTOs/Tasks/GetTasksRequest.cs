namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public enum TaskSortField
{
    Title,
    CategoryTitle,
    DueDate,
    Priority,
    IsCompleted,
    CreatedAt
}

public record GetTasksRequest(
    TaskSortField? SortBy = null, 
    bool IsDescending = false,

    bool? IsCompleted = null,
    int? Priority = null,
    Guid? CategoryId = null,
    string? SearchTerm = null,
    string? DueDate = null
);
