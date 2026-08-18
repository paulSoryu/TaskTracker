using TaskTracker.Shared.Enums;

namespace TaskTracker.Api.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    bool IsCompleted,
    DateTime CreatedAt,
    Guid? CategoryId,
    int Position
);
