using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record SortTasksQuery(
    TaskSortField SortBy,
    bool IsDescending
);