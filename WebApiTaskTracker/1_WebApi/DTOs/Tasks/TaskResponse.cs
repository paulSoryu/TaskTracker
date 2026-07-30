using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    Priority Priority,
    bool IsCompleted,
    DateTime CreatedAt,
    string CategoryTitle
);
