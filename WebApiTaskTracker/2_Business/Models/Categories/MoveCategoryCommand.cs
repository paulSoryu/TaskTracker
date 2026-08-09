namespace WebApiTaskTracker.Business.Models.Categories;

public record MoveCategoryCommand(
    Guid CategoryId,
    Guid TargetCategoryId
);
