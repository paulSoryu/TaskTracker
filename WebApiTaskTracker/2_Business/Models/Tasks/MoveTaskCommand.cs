namespace WebApiTaskTracker.Business.Models.Tasks;

public record MoveTaskCommand(
    Guid TaskId,
    int PageNumber,
    int PageSize,
    int NewLocalIndex
);