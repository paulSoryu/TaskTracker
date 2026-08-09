using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record GetPageByIdRequest(
    TaskSortField SortBy,
    bool IsDescending,

    int PageSize
)
{
    public class Validator : AbstractValidator<GetTasksRequest>
    {
        public Validator()
        {
            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(10).WithMessage("PageSize must be at least 10.")
                .LessThanOrEqualTo(100).WithMessage("PageSize must be at most 100.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");
        }
    }
}
