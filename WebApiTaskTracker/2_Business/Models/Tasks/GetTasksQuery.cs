using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record GetTasksQuery(
    TaskSortField? SortBy,
    bool IsDescending,

    bool? IsCompleted,
    int? Priority,
    Guid? CategoryId,
    string? SearchTerm,
    DateOnly? DueDate
);
