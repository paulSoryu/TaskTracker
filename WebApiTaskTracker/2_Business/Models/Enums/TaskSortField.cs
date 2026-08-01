using System.Text.Json.Serialization;

namespace WebApiTaskTracker.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskSortField
{
    Title,
    CategoryTitle,
    DueDate,
    Priority,
    IsCompleted,
    CreatedAt
}
