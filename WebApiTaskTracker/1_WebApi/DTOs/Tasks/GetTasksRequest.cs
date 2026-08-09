using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Tasks;

public record GetTasksRequest(
    // Sorting parameters
    TaskSortField SortBy = TaskSortField.Position,
    bool IsDescending = false,

    // Filter parameters
    string? SearchTerm = null,
    TaskPriority? Priority = null,
    bool? IsCompleted = null,

    Guid? CategoryId = null,
    bool? FilterByNoCategory = false,

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
                .GreaterThanOrEqualTo(10).WithMessage("PageSize must be at least 10.")
                .LessThanOrEqualTo(100).WithMessage("PageSize must be at most 100.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");

            RuleFor(x => x.DueDateFilterPreset)
                .IsInEnum().WithMessage("DueDateFilterPreset is not a valid option.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Priority is not a valid option.");

        }
    }
}
