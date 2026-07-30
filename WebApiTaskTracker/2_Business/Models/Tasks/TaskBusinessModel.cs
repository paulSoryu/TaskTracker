using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record TaskBusinessModel(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    Priority Priority,
    bool IsCompleted,
    DateTime CreatedAt,
    Guid CategoryId,
    string CategoryTitle
);