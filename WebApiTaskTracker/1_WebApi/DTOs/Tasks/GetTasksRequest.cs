using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record GetTasksRequest(
    // Sorting parameters
    TaskSortField? SortBy = null, 
    bool IsDescending = false,

    // Filter parameters
    string? SearchTerm = null,
    string? CategoryTitle = null,
    Priority? Priority = null,
    bool? IsCompleted = null,

    TaskDueDateFilterPreset? DueDateFilterPreset = null,
    DateOnly? SpecificMonth = null,

    // Pagination parameters
    int PageNumber = 1,
    int PageSize = 10
);
