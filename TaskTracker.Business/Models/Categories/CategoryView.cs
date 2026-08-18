using TaskTracker.Business.Models.Tasks;

namespace TaskTracker.Business.Models.Categories;

public record CategoryView(
    Guid Id,
    string Title,
    string Colour,
    int Position,
    IReadOnlyCollection<TaskView> Tasks
);