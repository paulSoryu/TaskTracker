namespace TaskTracker.Business.Models.Tasks;

public record MoveTaskCommand(
    Guid TaskId,
    Guid TargetTaskId
);