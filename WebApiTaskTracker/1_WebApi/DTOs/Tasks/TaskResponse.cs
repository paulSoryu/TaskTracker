using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    bool IsCompleted,
    DateTime CreatedAt,
    Guid? CategoryId
);
