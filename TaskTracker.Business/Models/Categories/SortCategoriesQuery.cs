using TaskTracker.Business.Models.Enums;

namespace TaskTracker.Business.Models.Categories;

public record SortCategoriesQuery(
    CategorySortField SortBy,
    bool IsDescending
);