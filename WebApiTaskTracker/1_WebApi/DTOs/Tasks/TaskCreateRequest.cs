using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.Utilities;

// this is a DTO for creating a task, including validation rules and a method to convert the DTO to a TaskEntity
// this breaks the single responsibility principle, as the DTO is responsible for data transfer and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record TaskCreateRequest(
    string Title,
    string Description,
    DateOnly? DueDate,
    TaskPriority Priority,
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
                .Must(BeTodayOrFuture).WithMessage("Date must be today or in the future.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority cannot be empty.")
                .IsInEnum().WithMessage("Priority must be between 1 and 3.");
        
        }

        private bool BeTodayOrFuture(DateOnly? date)
        {
            if (date is null) return true;
            return date.Value >= DateOnly.FromDateTime(DateTime.Today);
        }
    }
}
    
