namespace WebApiTaskTracker.WebApi.DTOs.Categories;

public record CategoryListResponse(
    Guid Id,
    string Title,
    int TaskCount,
    int CompletedTaskCount
);
