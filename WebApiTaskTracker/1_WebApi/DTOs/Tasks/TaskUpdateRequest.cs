using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.Utilities;

// this is a DTO for updating a task, including validation rules and a method to update a TaskEntity with the values from this DTO
// this breaks the single responsibility principle, as the DTO is responsible for data transfer and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskUpdateRequest(
    string Title,
    string Description,
    DateOnly? DueDate,
    Priority Priority,
    bool IsCompleted,
    string? CategoryTitle
)
{
    public class Validator : AbstractValidator<TaskUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .Length(TaskConstraints.TitleMinLength, TaskConstraints.TitleMaxLength).WithMessage($"Title must be between {TaskConstraints.TitleMinLength} and {TaskConstraints.TitleMaxLength} characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description cannot be empty.")
                .MaximumLength(TaskConstraints.DescriptionMaxLength).WithMessage($"Description must be at most {TaskConstraints.DescriptionMaxLength} characters.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority cannot be empty.");
        }
    }
}
