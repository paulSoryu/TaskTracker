using System.Text.Json.Serialization;

namespace TaskTracker.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CategorySortField
{
    Title,
    TaskCount,
    CompletedTaskCount,
    NotCompletedTaskCount,
    Position
}