using TaskTracker.Business.Models.Enums;
using TaskTracker.Shared.Enums;

namespace TaskTracker.Business.Models.Tasks;

public record FilterTasksQuery(
    string? SearchTerm,
    DateOnly? DueDate,
    bool? IsCompleted,
    TaskPriority? Priority,

    Guid? CategoryId,
    bool? FilterByNoCategory,

    TaskDueDateFilterPreset? DueDateFilterPreset,
    DateOnly? SpecificMonth
);