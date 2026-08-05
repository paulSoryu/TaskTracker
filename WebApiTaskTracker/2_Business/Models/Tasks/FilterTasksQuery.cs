using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record FilterTasksQuery(
    string? SearchTerm,
    DateOnly? DueDate,
    bool? IsCompleted,
    TaskPriority? Priority,

    Guid? CategoryId,
    bool? FilterByNoCategory,

    TaskDueDateFilterPreset? DueDateFilterPreset,
    DateOnly? SpecificMonth
) : ITaskQueryComponent;