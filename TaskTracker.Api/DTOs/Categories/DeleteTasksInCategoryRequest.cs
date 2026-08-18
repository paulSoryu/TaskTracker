namespace TaskTracker.Api.DTOs.Categories;

public record DeleteTasksInCategoryRequest(
    bool DeleteCompleted,
    bool DeleteNotCompleted
);
