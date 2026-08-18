using TaskTracker.Api.DTOs.Tasks;

namespace TaskTracker.Api.DTOs.Categories;

public record CategoryResponse(
    Guid Id,
    string Title,
    string Colour,
    List<TaskResponse> Tasks
);