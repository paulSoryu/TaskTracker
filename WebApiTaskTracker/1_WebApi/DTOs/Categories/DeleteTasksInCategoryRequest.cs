namespace WebApiTaskTracker.WebApi.DTOs.Categories;

public record DeleteTasksInCategoryRequest(
    bool DeleteCompleted,
    bool DeleteNotCompleted
);
