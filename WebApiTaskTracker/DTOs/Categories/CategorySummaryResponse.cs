namespace WebApiTaskTracker.DTOs.Categories;

public record CategorySummaryResponse(
    Guid Id,
    string Title,
    int TaskCount
);
