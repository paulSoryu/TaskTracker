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

    // Pagination parameters for the task list, used to determine the position of the new task
    int PageNumber,
    int PageSize
);