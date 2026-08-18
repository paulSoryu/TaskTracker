namespace TaskTracker.Api.DTOs.Categories;

public record CategoryListResponse(
    Guid Id,
    string Title,
    string Colour,
    int TaskCount,
    int CompletedTaskCount,
    int Position
);
