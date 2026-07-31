using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record GetTasksRequest(
    // Sorting parameters
    TaskSortField? SortBy = null,
    bool IsDescending = false,

    // Filter parameters
    string? SearchTerm = null,
    string? CategoryTitle = null,
    Priority? Priority = null,
    bool? IsCompleted = null,

    TaskDueDateFilterPreset? DueDateFilterPreset = null,
    DateOnly? SpecificMonth = null,

    // Pagination parameters
    int PageNumber = 1,
    int PageSize = 10
)
{
    public class Validator : AbstractValidator<GetTasksRequest>
    {
        public Validator()
        {
            RuleFor(x => x.PageSize)
                .LessThanOrEqualTo(1000).WithMessage("PageSize must be at most 1000.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");

            RuleFor(x => x.DueDateFilterPreset)
                .IsInEnum().WithMessage("DueDateFilterPreset is not a valid option.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Priority is not a valid option.");

        }
    }
}
