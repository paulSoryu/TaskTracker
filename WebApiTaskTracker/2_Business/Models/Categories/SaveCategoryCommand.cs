namespace WebApiTaskTracker.Business.Models.Categories;

public record SaveCategoryCommand(
    Guid Id,         
    string Title,
    string Colour
);
