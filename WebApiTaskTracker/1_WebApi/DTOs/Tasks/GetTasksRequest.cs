using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record GetTasksRequest(
    TaskSortField? SortBy = null, 
    bool IsDescending = false,

    // Filter parameters
    string? SearchTerm = null,
    string? CategoryTitle = null,
    DateOnly? DueDate = null,
    Priority? Priority = null,
    bool? IsCompleted = null
);
