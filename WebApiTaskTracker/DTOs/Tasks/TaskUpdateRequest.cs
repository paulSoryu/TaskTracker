using FluentValidation;
using WebApiTaskTracker.Data.Entities;
using WebApiTaskTracker.Utilities;

// this is a DTO for updating a task, including validation rules and a method to update a TaskEntity with the values from this DTO
// this breaks the single responsibility principle, as the DTO is responsible for data transfer, conversion and validation, but it is convenient for this simple app
namespace WebApiTaskTracker.DTOs.Tasks;

public record TaskUpdateRequest(
    string Title,
    string Description,
    DateOnly? DueDate,
    int Priority,
    Guid? CategoryId,
    string? CategoryName
)
{
    public void UpdateEntity(TaskEntity entity, Guid? categoryId)
    {
        entity.Title = this.Title;
        entity.Description = this.Description;
        entity.DueDate = this.DueDate;
        entity.Priority = this.Priority;
        entity.CategoryId = categoryId;
    }

    public class Validator : AbstractValidator<TaskUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .Length(TaskConstraints.TitleMinLength, TaskConstraints.TitleMaxLength).WithMessage($"Title must be between {TaskConstraints.TitleMinLength} and {TaskConstraints.TitleMaxLength} characters.");

            RuleFor(x => x.Description)
                .MaximumLength(TaskConstraints.DescriptionMaxLength).WithMessage($"Description must be at most {TaskConstraints.DescriptionMaxLength} characters.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Due date must be today or in the future.");

            RuleFor(x => x.Priority)
                .InclusiveBetween(TaskConstraints.PriorityMinValue, TaskConstraints.PriorityMaxValue).WithMessage($"Priority must be between {TaskConstraints.PriorityMinValue} and {TaskConstraints.PriorityMaxValue}.");
        }
    }
}
