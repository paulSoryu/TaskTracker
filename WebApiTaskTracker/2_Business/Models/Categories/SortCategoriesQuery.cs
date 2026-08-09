using WebApiTaskTracker.Business.Models.Enums;

namespace WebApiTaskTracker.WebApi.DTOs.Categories;

public record SortCategoriesQuery(
    CategorySortField SortBy,
    bool IsDescending
);