using WebApiTaskTracker.DataAccess.Entities;

// this is a DTO for returning a summary of task information, including a method to convert a TaskEntity to this DTO
// this breaks the single responsibility principle, as the DTO is responsible for both data transfer and conversion, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskSummaryResponse(
    Guid Id,
    string Title,
    string? DueDate,
    int Priority,
    string CategoryTitle
);
