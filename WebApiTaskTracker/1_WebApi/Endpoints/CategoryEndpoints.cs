using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.Business.Services.Categories;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.WebApi.Endpoints;

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

    private static async Task<Results<Ok<CategoryResponse>, NotFound<string>>> GetCategoryById(Guid id, ICategoryService categoryService)
    {
        CategoryBusinessModel? category = await categoryService.GetByIdAsync(id);

        if (category is null)
            return TypedResults.NotFound($"Category with ID {id} not found.");

        var response = category.Adapt<CategoryResponse>();
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<IReadOnlyCollection<CategorySummaryResponse>>> GetAllCategories(ICategoryService categoryService)
    {
        IReadOnlyCollection<CategoryBusinessModel> categories = await categoryService.GetAllAsync();

        var response = categories.Adapt<IReadOnlyCollection<CategorySummaryResponse>>();
        return TypedResults.Ok(response);
    }

    private static async Task<CreatedAtRoute<CategoryResponse>> CreateCategory(CategoryCreateRequest categoryRequest, ICategoryService categoryService, ClaimsPrincipal user)
    {
        var command = categoryRequest.Adapt<CategorySaveCommand>();

        CategoryBusinessModel createdCategory = await categoryService.CreateAsync(command, user.GetUserId());

        var response = createdCategory.Adapt<CategoryResponse>();
        return TypedResults.CreatedAtRoute(response, "GetCategoryById", new { id = response.Id });
    }

    private static async Task<NoContent> UpdateCategory(Guid id, CategoryUpdateRequest categoryRequest, ICategoryService categoryService)
    {
        var command = categoryRequest.Adapt<CategorySaveCommand>() with { Id = id };

        await categoryService.UpdateAsync(command);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteCategory(Guid id, ICategoryService categoryService)
    {
        await categoryService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
