namespace WebApiTaskTracker.WebApi.DTOs.Categories;

public record CategorySummaryResponse(
    Guid Id,
    string Title,
    int TaskCount,
    int CompletedTaskCount
);
