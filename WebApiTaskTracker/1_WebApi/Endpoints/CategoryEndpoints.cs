using FluentResults;
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

    private static async Task<Results<Ok<CategoryResponse>, ProblemHttpResult>> GetCategoryById(Guid id, ICategoryService categoryService)
    {
        Result<CategoryBusinessModel> result = await categoryService.GetByIdAsync(id);

        Result<CategoryResponse> responseResult = result.Map(category => category.Adapt<CategoryResponse>());

        return responseResult.ToTypedHttpResult();
    }

    private static async Task<Ok<IReadOnlyCollection<CategorySummaryResponse>>> GetAllCategories(ICategoryService categoryService)
    {
        IReadOnlyCollection<CategoryBusinessModel> categories = await categoryService.GetAllAsync();

        var response = categories.Adapt<IReadOnlyCollection<CategorySummaryResponse>>();
        return TypedResults.Ok(response);
    }

    private static async Task<Results<CreatedAtRoute<CategoryResponse>, ProblemHttpResult>> CreateCategory(CategoryCreateRequest categoryRequest, ICategoryService categoryService, ClaimsPrincipal user)
    {
        var command = categoryRequest.Adapt<CategorySaveCommand>();

        Result<CategoryBusinessModel> result = await categoryService.CreateAsync(command, user.GetUserId());

        Result<CategoryResponse> responseResult = result.Map(category => category.Adapt<CategoryResponse>());

        return responseResult.ToCreatedAtRouteResult(
            routeName: "GetCategoryById",
            routeValues: new { id = responseResult.ValueOrDefault?.Id });
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateCategory(Guid id, CategoryUpdateRequest categoryRequest, ICategoryService categoryService)
    {
        var command = categoryRequest.Adapt<CategorySaveCommand>() with { Id = id };

        Result result = await categoryService.UpdateAsync(command);

        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteCategory(Guid id, ICategoryService categoryService)
    {
        Result result = await categoryService.DeleteAsync(id);

        return result.ToTypedHttpResult();
    }
}
