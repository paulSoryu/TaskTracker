using System.Text.Json.Serialization;

namespace TaskTracker.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskDueDateFilterPreset
{
    All,
    Overdue,
    Today,
    ThisWeek,
    ThisMonth,
    NoDueDate
}
