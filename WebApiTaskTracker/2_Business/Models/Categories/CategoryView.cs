using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Models.Categories;

public record CategoryView(
    Guid Id,
    string Title,
    string Colour,
    IReadOnlyCollection<TaskView> Tasks
);