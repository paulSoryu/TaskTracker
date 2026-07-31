using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.Business.Models.Categories;

public record GetCategoriesQuery(
    // Sorting parameters
    CategorySortField? SortBy,
    bool IsDescending,

    // Filter parameters
    string? SearchTerm
);
