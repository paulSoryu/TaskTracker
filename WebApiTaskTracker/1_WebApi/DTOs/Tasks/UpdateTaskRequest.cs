using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;
using WebApiTaskTracker.Utilities;

// this is a DTO for updating a task, including validation rules and a method to update a TaskEntity with the values from this DTO
// this breaks the single responsibility principle, as the DTO is responsible for data transfer and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    bool IsCompleted,
    Guid? CategoryId
)
{
    public class Validator : AbstractValidator<UpdateTaskRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .Length(TaskConstraints.TitleMinLength, TaskConstraints.TitleMaxLength).WithMessage($"Title must be between {TaskConstraints.TitleMinLength} and {TaskConstraints.TitleMaxLength} characters.");

            RuleFor(x => x.Description)
                .MaximumLength(TaskConstraints.DescriptionMaxLength).WithMessage($"Description must be at most {TaskConstraints.DescriptionMaxLength} characters.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority cannot be empty.")
                .IsInEnum().WithMessage("Priority must be between 1 and 3.");
        }
    }
}
