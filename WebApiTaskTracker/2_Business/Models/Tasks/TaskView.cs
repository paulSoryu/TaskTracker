using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record TaskView(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    bool IsCompleted,
    DateTime CreatedAt,
    Guid CategoryId,
    string CategoryTitle
);