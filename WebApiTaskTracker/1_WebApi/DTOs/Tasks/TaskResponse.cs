using WebApiTaskTracker.DataAccess.Entities;

// this is a DTO for returning task information, including a method to convert a TaskEntity to this DTO
// this breaks the single responsibility principle, as the DTO is responsible for both data transfer and conversion, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    int Priority,
    string CategoryTitle
)
{
    public static TaskResponse FromEntity(TaskEntity entity)
    {
        return new TaskResponse(
            Id: entity.Id,
            Title: entity.Title,
            Description: entity.Description ?? "",
            CategoryTitle: entity.Category?.Title ?? "",
            DueDate: entity.DueDate,
            Priority: entity.Priority
        );
    }
}
