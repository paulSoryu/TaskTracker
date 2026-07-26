using FluentValidation;
using WebApiTaskTracker.Utilities;

// this is a DTO for updating a category, including validation rules
// this breaks the single responsibility principle, as the DTO is responsible for data transfer and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Categories;

public record CategoryUpdateRequest(
    string Title,
    string Colour
)
{
    public class Validator : AbstractValidator<CategoryUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MinimumLength(CategoryConstraints.TitleMinLength).WithMessage($"Title must be at least {CategoryConstraints.TitleMinLength} characters.")
                .MaximumLength(CategoryConstraints.TitleMaxLength).WithMessage($"Title must be at most {CategoryConstraints.TitleMaxLength} characters.");

            RuleFor(x => x.Colour)
                .NotEmpty().WithMessage("Color is required.")
                .Matches(@"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")
                .WithMessage("Invalid color format. Use #RRGGBB or #AARRGGBB.");
        }
    }
}
