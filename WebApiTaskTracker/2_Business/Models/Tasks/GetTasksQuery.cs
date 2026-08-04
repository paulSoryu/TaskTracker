using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record GetTasksQuery(
    // Sorting parameters
    TaskSortField? SortBy,
    bool IsDescending,

    // Filter parameters
    string? SearchTerm,
    DateOnly? DueDate,
    bool? IsCompleted,
    TaskPriority? Priority,
    
    Guid? CategoryId,
    bool FilterByNoCategory,

    TaskDueDateFilterPreset? DueDateFilterPreset,
    DateOnly? SpecificMonth,

    // Pagination parameters
    int PageNumber,
    int PageSize
);
