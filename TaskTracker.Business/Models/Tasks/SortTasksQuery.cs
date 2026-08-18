using TaskTracker.Business.Models.Enums;

namespace TaskTracker.Business.Models.Tasks;

public record SortTasksQuery(
    TaskSortField SortBy,
    bool IsDescending
);