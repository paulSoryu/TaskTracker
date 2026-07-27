using Mapster;
using System.Security.Claims;
using WebApiTaskTracker.Business.Models.Categories;
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

    private static async Task<IResult> GetCategoryById(Guid id, ICategoryService categoryService)
    {
        CategoryBusinessModel? category = await categoryService.GetByIdAsync(id);

        if (category is null)
            return Results.NotFound(new { Message = $"Category with ID {id} not found." });
        
        var response = category.Adapt<CategoryResponse>();
        return Results.Ok(response);
    }

    private static async Task<IResult> GetAllCategories(ICategoryService categoryService)
    {
        IReadOnlyCollection<CategoryBusinessModel> categories = await categoryService.GetAllAsync();

        var response = categories.Adapt<IReadOnlyCollection<CategorySummaryResponse>>();
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateCategory(CategoryCreateRequest categoryRequest, ICategoryService categoryService, ClaimsPrincipal user)
    {
        var command = categoryRequest.Adapt<CategorySaveCommand>();

        CategoryBusinessModel createdCategory = await categoryService.CreateAsync(command, user.GetUserId());

        var response = createdCategory.Adapt<CategoryResponse>();
        return Results.CreatedAtRoute("GetCategoryById", new { id = response.Id }, response);
    }

    private static async Task<IResult> UpdateCategory(Guid id, CategoryUpdateRequest categoryRequest, ICategoryService categoryService)
    {
        var command = categoryRequest.Adapt<CategorySaveCommand>() with { Id = id };

        await categoryService.UpdateAsync(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCategory(Guid id, ICategoryService categoryService)
    {
        await categoryService.DeleteAsync(id);
        return Results.NoContent();
    }
}
