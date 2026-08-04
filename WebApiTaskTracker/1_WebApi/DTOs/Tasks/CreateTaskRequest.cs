using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.Utilities;

// this is a DTO for creating a task, including validation rules and a method to convert the DTO to a TaskEntity
// this breaks the single responsibility principle, as the DTO is responsible for data transfer and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    Guid? CategoryId,

    // Pagination parameters for the task list, used to determine the position of the new task
    int PageNumber,
    int PageSize
)
{
    public class Validator : AbstractValidator<CreateTaskRequest>
    {
        public Validator()
        {
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize must be at most 100.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .Length(TaskConstraints.TitleMinLength, TaskConstraints.TitleMaxLength).WithMessage($"Title must be between {TaskConstraints.TitleMinLength} and {TaskConstraints.TitleMaxLength} characters.");

            RuleFor(x => x.Description)
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
    
