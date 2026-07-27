namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskSummaryResponse(
    Guid Id,
    string Title,
    string? DueDate,
    int Priority,
    string CategoryTitle
);
