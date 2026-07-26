using WebApiTaskTracker.DTOs.Tasks;

namespace WebApiTaskTracker.DTOs.Categories;

public record CategoryResponse(
    Guid Id,
    string Title,
    string Colour,
    List<TaskResponse> Tasks
);