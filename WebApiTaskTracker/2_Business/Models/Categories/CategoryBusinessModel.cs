using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Models.Categories;

public record CategoryBusinessModel(
    Guid Id,
    string Title,
    string Colour,
    IReadOnlyCollection<TaskBusinessModel> Tasks
);