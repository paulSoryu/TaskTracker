using System.Text.Json.Serialization;

namespace WebApiTaskTracker.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CategorySortField
{
    Title,
    TaskCount,
    CompletedTaskCount,
    NotCompletedTaskCount,
    Position
}