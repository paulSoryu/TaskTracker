using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record GetTasksQuery(
    TaskSortField? SortBy,
    bool IsDescending,

    // Filter parameters
    string? SearchTerm,
    string? CategoryTitle,
    DateOnly? DueDate,
    bool? IsCompleted,
    Priority? Priority
);
