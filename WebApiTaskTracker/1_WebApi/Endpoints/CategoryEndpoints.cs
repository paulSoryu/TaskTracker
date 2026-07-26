using System.Security.Claims;
using WebApiTaskTracker.Business.Services.Categories;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.WebApi.Endpoints;

// By passing DTOs directly to the service layer, we are breaking the single responsibility principle, as the service layer is now responsible for both business logic and data transfer.
// However, this is a common practice in simple applications to reduce boilerplate code and improve maintainability.
// In a more complex application, it would be better to use separate DTOs for the service layer and the API layer, and map between them using a mapping library like Mapster.
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
