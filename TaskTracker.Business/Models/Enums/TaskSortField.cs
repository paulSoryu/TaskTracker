using System.Text.Json.Serialization;

namespace TaskTracker.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskSortField
{
    Title,
    CategoryTitle,
    DueDate,
    Priority,
    IsCompleted,
    CreatedAt,
    Position
}
