using FluentValidation;
using TaskTracker.Business.Models.Enums;

namespace TaskTracker.Api.DTOs.Tasks;

public record GetPageByIdRequest(
    TaskSortField SortBy,
    bool IsDescending,

    int PageSize
)
{
    public class Validator : AbstractValidator<GetPageByIdRequest>
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
