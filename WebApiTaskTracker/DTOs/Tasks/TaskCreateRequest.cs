using FluentValidation;
using WebApiTaskTracker.Data.Entities;
using WebApiTaskTracker.Utilities;

// this is a DTO for creating a task, including validation rules and a method to convert the DTO to a TaskEntity
// this breaks the single responsibility principle, as the DTO is responsible for data transfer, conversion and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.DTOs.Tasks;

public record TaskCreateRequest(
    string Title,
    string Description,
    DateOnly? DueDate,
    int Priority,
    string CategoryTitle
)
{
    public class Validator : AbstractValidator<TaskCreateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MinimumLength(TaskConstraints.TitleMinLength).WithMessage($"Title must be at least {TaskConstraints.TitleMinLength} characters.")
                .MaximumLength(TaskConstraints.TitleMaxLength).WithMessage($"Title must be at most {TaskConstraints.TitleMaxLength} characters.");

            RuleFor(x => x.Description)
                .MaximumLength(TaskConstraints.DescriptionMaxLength).WithMessage($"Description must be at most {TaskConstraints.DescriptionMaxLength} characters.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Due date must be today or in the future.");

            RuleFor(x => x.Priority)
                .InclusiveBetween(TaskConstraints.PriorityMinValue, TaskConstraints.PriorityMaxValue).WithMessage($"Priority must be between {TaskConstraints.PriorityMinValue} and {TaskConstraints.PriorityMaxValue}.");
        }
    }
}



