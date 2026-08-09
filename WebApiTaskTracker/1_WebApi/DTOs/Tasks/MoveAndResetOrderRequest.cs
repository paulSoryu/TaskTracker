using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record MoveAndResetOrderRequest(
    Guid TaskId,
    Guid TargetTaskId,

    TaskSortField SortBy,
    bool IsDescending
)
{
    public class Validator : AbstractValidator<MoveAndResetOrderRequest>
    {
        public Validator()
        {
            RuleFor(x => x.TaskId)
                .NotEmpty().WithMessage("TaskId is required.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy must be a valid TaskSortField value.");

        }
    }
}
