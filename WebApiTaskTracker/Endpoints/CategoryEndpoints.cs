using System.Security.Claims;
using WebApiTaskTracker.DTOs;
using WebApiTaskTracker.DTOs.Categories;
using WebApiTaskTracker.Services.Categories;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var routeGroup = endpoints.MapGroup("/api/categories").RequireAuthorization();

        routeGroup.MapGet("/", GetAllCategories);

        routeGroup.MapGet("/{id:Guid}", GetCategoryById)
            .WithName("GetCategoryById");

        routeGroup.MapPost("/", CreateCategory)
            .AddEndpointFilter<ValidationFilter<CategoryCreateRequest>>();

        routeGroup.MapPut("/{id:Guid}", UpdateCategory)
            .AddEndpointFilter<ValidationFilter<CategoryUpdateRequest>>();

        routeGroup.MapDelete("/{id:Guid}", DeleteCategory);
    }

    private static async Task<IResult> GetAllCategories(ICategoryService categoryService)
    {
        var categories = await categoryService.GetAllAsync();
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetCategoryById(Guid id, ICategoryService categoryService)
    {
        var category = await categoryService.GetByIdAsync(id);
        return Results.Ok(category);
    }

    private static async Task<IResult> CreateCategory(CategoryCreateRequest categoryRequest, ICategoryService categoryService, ClaimsPrincipal user)
    {
        CategoryResponse createdCategory = await categoryService.CreateAsync(categoryRequest, user.GetUserId());
        return Results.CreatedAtRoute("GetCategoryById", new { id = createdCategory.Id }, createdCategory);
    }

    private static async Task<IResult> UpdateCategory(Guid id, CategoryUpdateRequest categoryRequest, ICategoryService categoryService)
    {
        await categoryService.UpdateAsync(id, categoryRequest);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCategory(Guid id, ICategoryService categoryService)
    {
        await categoryService.DeleteAsync(id);
        return Results.NoContent();
    }
}
