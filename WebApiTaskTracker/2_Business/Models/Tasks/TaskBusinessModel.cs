namespace WebApiTaskTracker.Business.Models.Tasks;

public record TaskBusinessModel(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    int Priority,
    string CategoryTitle
);