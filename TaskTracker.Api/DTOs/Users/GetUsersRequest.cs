using FluentValidation;
using TaskTracker.Business.Models.Enums;
using TaskTracker.Shared.Constants;

namespace TaskTracker.Api.DTOs.Users;

public record GetUsersRequest(

    // Sorting parameters
    UserSortField SortBy = UserSortField.CreatedAt,
    bool IsDescending = false,

    // Filter parameters
    string? SearchTerm = null,

    // Pagination parameters
    int PageNumber = 1,
    int PageSize = 10
)
{
    public class Validator : AbstractValidator<GetUsersRequest>
    {
        public Validator()
        {
            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(PaginationConstraints.PageMinSize).WithMessage($"PageSize must be at least {PaginationConstraints.PageMinSize}.")
                .LessThanOrEqualTo(PaginationConstraints.PageMaxSize).WithMessage($"PageSize must be at most {PaginationConstraints.PageMaxSize}.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");

        }
    }
}
