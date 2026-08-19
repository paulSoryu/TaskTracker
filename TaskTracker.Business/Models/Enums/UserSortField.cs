using System.Text.Json.Serialization;

namespace TaskTracker.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserSortField
{
    Email,
    CreatedAt,
    TaskCount,
    CompletedTaskCount,
    NotCompletedTaskCount,
    CategoryCount
}
