namespace WebApiTaskTracker.Business.Models.Tasks;

public record TaskSaveCommand(
    Guid? Id,                 
    string Title,
    string Description,
    DateOnly? DueDate,        
    int Priority,
    Guid? CategoryId,         
    string? CategoryTitle     
);