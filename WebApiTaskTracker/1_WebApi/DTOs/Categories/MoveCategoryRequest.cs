using FluentValidation;
using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Categories;

public record MoveCategoryRequest(
    Guid CategoryId,
    Guid TargetCategoryId,

    CategorySortField SortBy,
    bool IsDescending
)
{
    public class Validator : AbstractValidator<MoveCategoryRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("TaskId is required.");

            RuleFor(x => x.TargetCategoryId)
                .NotEmpty().WithMessage("TargetTaskId is required.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");
        }
    }
}
