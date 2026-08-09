using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Tasks;

public record SaveTaskCommand(
    Guid? Id,                 
    string Title,
    string Description,
    DateOnly? DueDate,        
    TaskPriority Priority,
    bool IsCompleted,
    Guid? CategoryId,

    Guid? FirstVisibleTaskIdOnPage
);