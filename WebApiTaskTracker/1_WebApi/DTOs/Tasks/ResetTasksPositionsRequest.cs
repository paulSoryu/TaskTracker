using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record ResetTasksPositionsRequest(
    Guid TaskId,
    int PageNumber,
    int PageSize,
    int NewLocalIndex,

    TaskSortField SortBy,
    bool IsDescending
)
{
    public class Validator : AbstractValidator<ResetTasksPositionsRequest>
    {
        public Validator()
        {
            RuleFor(x => x.TaskId)
                .NotEmpty().WithMessage("TaskId is required.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize must be at most 100.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.NewLocalIndex)
                .GreaterThan(0).WithMessage("NewLocalIndex must be greater than to 0.")
                .LessThanOrEqualTo(x => x.PageSize).WithMessage("NewLocalIndex must be less than or equal to PageSize.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy must be a valid TaskSortField value.");

        }
    }
}
