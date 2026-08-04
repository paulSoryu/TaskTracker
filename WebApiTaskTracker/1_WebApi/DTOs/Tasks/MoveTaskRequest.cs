using FluentValidation;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record MoveTaskRequest(
    Guid TaskId,
    int PageNumber,
    int PageSize,
    int NewLocalIndex
)
{
    public class Validator : AbstractValidator<MoveTaskRequest>
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
                .GreaterThanOrEqualTo(0).WithMessage("NewLocalIndex must be greater than or equal to 0.");

        }
    }
}
