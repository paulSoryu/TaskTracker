namespace WebApiTaskTracker.Business.Models.Categories;

public record CategorySaveCommand(
    Guid? Id,         
    string Title,
    string Colour
);
