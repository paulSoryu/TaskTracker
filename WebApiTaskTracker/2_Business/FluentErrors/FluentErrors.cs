using FluentResults;
using Microsoft.AspNetCore.Identity;
using System.Collections;

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

public class CategoryLimitExceededError : Error
{
    public int MaxAllowedCategories { get; }
    public bool IsEmailConfirmed { get; }

    public CategoryLimitExceededError(int maxAllowedCategories, bool isEmailConfirmed)
        : base(BuildMessage(maxAllowedCategories, isEmailConfirmed))
    {
        MaxAllowedCategories = maxAllowedCategories;
        IsEmailConfirmed = isEmailConfirmed;

        Metadata.Add("ErrorCode", "CATEGORY_LIMIT_EXCEEDED");
        Metadata.Add("MaxLimit", maxAllowedCategories);
    }

    private static string BuildMessage(int maxAllowedCategories, bool isEmailConfirmed)
    {
        return isEmailConfirmed
            ? $"You have reached the category limit ({maxAllowedCategories}). Creating new categories is not possible."
            : $"You have reached the category limit ({maxAllowedCategories}). Confirm your email to increase the limit to 100.";
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

public class IdentityValidationError : Error
{
    public Dictionary<string, string[]> Errors { get; }

    public IdentityValidationError(string message, IEnumerable<IdentityError> identityErrors)
        : base(message)
    {
        Metadata.Add("ErrorCode", "IDENTITY_VALIDATION_ERROR");

        Errors = identityErrors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray()
            );

    }
}

public class ReorderingError : Error
{
    public string EntityName { get; }
    public object? EntityId { get; } = null;
    public int OldPosition { get; } = -1;
    public int NewPosition { get; } = -1;
    public string ExceptionMessage { get; } = String.Empty;

    public ReorderingError(string entityName, int newPosition)
        : base($"Failed to move {entityName} to position {newPosition}")
    {
        EntityName = entityName;
        NewPosition = newPosition;
        Metadata.Add("ErrorCode", "REORDER_FAILED");
        Metadata.Add("EntityName", entityName);
        Metadata.Add("NewPosition", newPosition);
    }

    public ReorderingError(string entityName, object entityId, int oldPosition)
        : base($"Failed to remove {entityName} from position {oldPosition}")
    {
        EntityName = entityName;
        OldPosition = oldPosition;
        Metadata.Add("ErrorCode", "REORDER_FAILED");
        Metadata.Add("EntityName", entityName);
        Metadata.Add("OldPosition", oldPosition);
    }

    public ReorderingError(string entityName, object entityId, int oldPosition, int newPosition)
        : base($"Failed to move {entityName} with ID '{entityId}' from position {oldPosition} to position {newPosition}")
    {
        EntityName = entityName;
        EntityId = entityId;
        OldPosition = oldPosition;
        NewPosition = newPosition;

        Metadata.Add("ErrorCode", "REORDER_FAILED");
        Metadata.Add("EntityName", entityName);
        Metadata.Add("EntityId", entityId.ToString());
        Metadata.Add("OldPosition", oldPosition);
        Metadata.Add("NewPosition", newPosition);
    }

    public ReorderingError(string entityName, object entityId, int oldPosition, int newPosition, string exceptionMessage)
        : base($"Failed to move {entityName} with ID '{entityId}' from position {oldPosition} to position {newPosition}: {exceptionMessage}")
    {
        EntityName = entityName;
        EntityId = entityId;
        OldPosition = oldPosition;
        NewPosition = newPosition;

        Metadata.Add("ErrorCode", "REORDER_FAILED");
        Metadata.Add("EntityName", entityName);
        Metadata.Add("EntityId", entityId.ToString());
        Metadata.Add("OldPosition", oldPosition);
        Metadata.Add("NewPosition", newPosition);
        Metadata.Add("ExceptionMessage", exceptionMessage);
    }
}