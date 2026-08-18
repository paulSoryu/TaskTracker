using FluentValidation;
using TaskTracker.Business.Models.Enums;

namespace TaskTracker.Api.DTOs.Tasks;

public record MoveTaskRequest(
    Guid TaskId,
    Guid TargetTaskId,

    TaskSortField SortBy,
    bool IsDescending
)
{
    public class Validator : AbstractValidator<MoveTaskRequest>
    {
        public Validator()
        {
            RuleFor(x => x.TaskId)
                .NotEmpty().WithMessage("TaskId is required.");

            RuleFor(x => x.TargetTaskId)
                .NotEmpty().WithMessage("TargetTaskId is required.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");
        }
    }
}
