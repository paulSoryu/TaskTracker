using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.WebApi.DTOs.Categories;

public record CategoryResponse(
    Guid Id,
    string Title,
    string Colour,
    List<TaskResponse> Tasks
);