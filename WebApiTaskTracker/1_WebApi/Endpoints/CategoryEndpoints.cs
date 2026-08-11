using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        var routeGroup = endpoints.MapGroup("/api/categories")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        routeGroup.MapGet("/", GetAllCategories)
            .WithValidation<GetCategoriesRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup.MapGet("/{id:Guid}", GetCategoryById)
            .WithName("GetCategoryById")
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapPost("/", CreateCategory)
            .WithValidation<CreateCategoryRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup.MapPut("/{id:Guid}", UpdateCategory)
            .WithValidation<UpdateCategoryRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup.MapDelete("/{id:Guid}", DeleteCategory)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapPatch("/move", MoveCategory)
            .WithValidation<MoveCategoryRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapDelete("/{id:Guid}/tasks", DeleteTasksInCategory)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<IReadOnlyCollection<CategoryListResponse>>> GetAllCategories(ICategoryService categoryService, [AsParameters] GetCategoriesRequest request)
    {
        var filterQuery = request.Adapt<FilterCategoriesQuery>();
        var sortQuery = request.Adapt<SortCategoriesQuery>();
        IReadOnlyCollection<CategoryView> categories = await categoryService.GetAllAsync(filterQuery, sortQuery);

        var response = categories.Adapt<IReadOnlyCollection<CategoryListResponse>>();
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<CategoryResponse>, ProblemHttpResult>> GetCategoryById(Guid id, ICategoryService categoryService)
    {
        Result<CategoryView> result = await categoryService.GetByIdAsync(id);

        Result<CategoryResponse> responseResult = result.Map(category => category.Adapt<CategoryResponse>());

        return responseResult.ToTypedHttpResult();
    }

    private static async Task<Results<CreatedAtRoute<CategoryResponse>, ProblemHttpResult>> CreateCategory(CreateCategoryRequest request, ICategoryService categoryService, ClaimsPrincipal user)
    {
        var command = request.Adapt<SaveCategoryCommand>();
        var query = request.Adapt<SortCategoriesQuery>();

        Result<CategoryView> result = await categoryService.CreateAsync(command, query, user.GetUserId());

        Result<CategoryResponse> responseResult = result.Map(category => category.Adapt<CategoryResponse>());

        return responseResult.ToCreatedAtRouteResult(
            routeName: "GetCategoryById",
            routeValues: new { id = responseResult.ValueOrDefault?.Id });
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateCategory(Guid id, UpdateCategoryRequest request, ICategoryService categoryService)
    {
        var command = request.Adapt<SaveCategoryCommand>() with { Id = id };

        Result result = await categoryService.UpdateAsync(command);

        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteCategory(Guid id, ICategoryService categoryService)
    {
        Result result = await categoryService.DeleteAsync(id);

        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> MoveCategory(MoveCategoryRequest request, ICategoryService categoryService)
    {
        var command = request.Adapt<MoveCategoryCommand>();
        var query = request.Adapt<SortCategoriesQuery>();
        Result result = await categoryService.MoveAsync(command, query);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteTasksInCategory(Guid id, [FromBody]DeleteTasksInCategoryRequest request, ICategoryService categoryService)
    {
        Result result = await categoryService.DeleteTasksByCategoryIdAsync(id, request.DeleteCompleted, request.DeleteNotCompleted);
        return result.ToTypedHttpResult();
    }
}
