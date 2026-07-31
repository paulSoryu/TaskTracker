using FluentResults;

namespace WebApiTaskTracker.Business.FluentErrors;

public class NotFoundError : Error
{
    public string EntityName { get; }
    public object EntityId { get; }

    public NotFoundError(string entityName, object entityId)
        : base($"{entityName} with ID '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;

        Metadata.Add("ErrorCode", "RESOURCE_NOT_FOUND");
        Metadata.Add("EntityName", entityName);
        Metadata.Add("EntityId", entityId.ToString() ?? string.Empty);
    }
}

public class TaskLimitExceededError : Error
{
    public int MaxAllowedTasks { get; }
    public bool IsEmailConfirmed { get; }

    public TaskLimitExceededError(int maxAllowedTasks, bool isEmailConfirmed)
        : base(BuildMessage(maxAllowedTasks, isEmailConfirmed))
    {
        MaxAllowedTasks = maxAllowedTasks;
        IsEmailConfirmed = isEmailConfirmed;

        Metadata.Add("ErrorCode", "TASK_LIMIT_EXCEEDED");
        Metadata.Add("MaxLimit", maxAllowedTasks);
    }

    private static string BuildMessage(int maxAllowedTasks, bool isEmailConfirmed)
    {
        return isEmailConfirmed
            ? $"You have reached the task limit ({maxAllowedTasks}). Creating new tasks is not possible."
            : $"You have reached the task limit ({maxAllowedTasks}). Confirm your email to increase the limit to 1000.";
    }
}

public class ValidationError : Error
{
    public Dictionary<string, string[]> Errors { get; } = new();

    public ValidationError(string message) : base(message)
    {
        Metadata.Add("ErrorCode", "BUSINESS_VALIDATION_ERROR");
    }

    public ValidationError(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
        Metadata.Add("ErrorCode", "BUSINESS_VALIDATION_ERROR");
    }
}