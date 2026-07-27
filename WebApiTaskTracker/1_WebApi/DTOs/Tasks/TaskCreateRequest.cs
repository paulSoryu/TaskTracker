using FluentValidation;
using WebApiTaskTracker.Utilities;

// this is a DTO for creating a task, including validation rules and a method to convert the DTO to a TaskEntity
// this breaks the single responsibility principle, as the DTO is responsible for data transfer, conversion and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskCreateRequest(
    string Title,
    string Description,
    string? DueDate,
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
                .Length(TaskConstraints.TitleMinLength, TaskConstraints.TitleMaxLength).WithMessage($"Title must be between {TaskConstraints.TitleMinLength} and {TaskConstraints.TitleMaxLength} characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description cannot be empty.")
                .MaximumLength(TaskConstraints.DescriptionMaxLength).WithMessage($"Description must be at most {TaskConstraints.DescriptionMaxLength} characters.");

            RuleFor(x => x.DueDate)
                .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Format must be YYYY-MM-DD.")
                .Must(BeAValidCalendarDate).WithMessage("Invalid date.")
                .Must(BeTodayOrFuture).WithMessage("Date must be today or in the future.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority cannot be empty.")
                .InclusiveBetween(TaskConstraints.PriorityMinValue, TaskConstraints.PriorityMaxValue).WithMessage($"Priority must be between {TaskConstraints.PriorityMinValue} and {TaskConstraints.PriorityMaxValue}.");
        }

        private bool BeAValidCalendarDate(string dateStr)
        {
            return DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out _);
        }

        private bool BeTodayOrFuture(string dateStr)
        {
            if (DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out DateOnly parsedDate))
            {
                return parsedDate >= DateOnly.FromDateTime(DateTime.Today);
            }
            return false;
        }
    }
}



