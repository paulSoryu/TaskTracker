using FluentValidation;
using TaskTracker.Business.Models.Enums;
using TaskTracker.Shared.Constants;

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
                .GreaterThanOrEqualTo(PaginationConstraints.PageMinSize).WithMessage($"PageSize must be at least {PaginationConstraints.PageMinSize}.")
                .LessThanOrEqualTo(PaginationConstraints.PageMaxSize).WithMessage($"PageSize must be at most {PaginationConstraints.PageMaxSize}.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");
        }
    }
}
