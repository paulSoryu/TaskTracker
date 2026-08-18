using FluentValidation;
using TaskTracker.Business.Models.Enums;

namespace TaskTracker.Api.DTOs.Categories;

public record GetCategoriesRequest(
    // Sorting parameters
    CategorySortField SortBy = CategorySortField.Position,
    bool IsDescending = false,

    // Filter parameters
    string? SearchTerm = null
)
{
    public class Validator : AbstractValidator<GetCategoriesRequest>
    {
        public Validator()
        {
            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("SortBy is not a valid option.");

        }
    }
}

