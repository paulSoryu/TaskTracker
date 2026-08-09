using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using WebApiTaskTracker.Business.Models;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.Business.Services.Tasks;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs;
using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.WebApi.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var routeGroup = endpoints.MapGroup("/api/tasks")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        routeGroup.MapGet("/", GetAllTasks);

        routeGroup.MapGet("/{id:Guid}", GetTaskById)
            .WithName("GetTaskById")
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapGet("{id:Guid}/page", GetPageById)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapPost("/", CreateTask)
            .WithValidation<CreateTaskRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup.MapPut("/{id:Guid}", UpdateTask)
            .WithValidation<UpdateTaskRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup.MapDelete("/{id:Guid}", DeleteTask)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapPatch("/move", MoveTask)
            .WithValidation<MoveTaskRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<PagedResult<TaskListResponse>>> GetAllTasks(ITaskService taskService, [AsParameters] GetTasksRequest request)
    {
        var filterQuery = request.Adapt<FilterTasksQuery>();
        var sortQuery = request.Adapt<SortTasksQuery>();
        var paginationQuery = request.Adapt<PaginateTasksQuery>();
        PagedResult<TaskView> pagedTasks = await taskService.GetAllAsync(filterQuery, sortQuery, paginationQuery);

        var response = new PagedResult<TaskListResponse>(
            pagedTasks.Items.Adapt<IReadOnlyCollection<TaskListResponse>>(),
            pagedTasks.TotalCount);

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<TaskResponse>, ProblemHttpResult>> GetTaskById(Guid id, ITaskService taskService)
    {
        Result<TaskView> result = await taskService.GetByIdAsync(id);

        Result<TaskResponse> response = result.Map(task => task.Adapt<TaskResponse>());

        return response.ToTypedHttpResult();
    }

    private static async Task<Results<Ok<TaskPageResponse>, ProblemHttpResult>> GetPageById(Guid id, ITaskService taskService, [AsParameters] GetPageByIdRequest taskRequest)
    {
        var query = taskRequest.Adapt<SortTasksQuery>();
        int pageSize = taskRequest.PageSize;

        Result<int> result = await taskService.GetPageById(id, query, pageSize);
        Result<TaskPageResponse> response = new TaskPageResponse(result.Value);

        return response.ToTypedHttpResult();
    }

    private static async Task<Results<CreatedAtRoute<TaskResponse>, ProblemHttpResult>> CreateTask(CreateTaskRequest taskRequest, ITaskService taskService, ClaimsPrincipal user)
    {
        var command = taskRequest.Adapt<SaveTaskCommand>();
        var query = taskRequest.Adapt<SortTasksQuery>();
        Guid userId = user.GetUserId();

        Result<TaskView> result = await taskService.CreateAsync(command, query, userId);

        Result<TaskResponse> responseResult = result.Map(task => task.Adapt<TaskResponse>());

        return responseResult.ToCreatedAtRouteResult(
            routeName: "GetTaskById",
            routeValues: new { id = responseResult.ValueOrDefault?.Id });
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateTask(Guid id, UpdateTaskRequest taskRequest, ITaskService taskService)
    {
        var command = taskRequest.Adapt<SaveTaskCommand>() with { Id = id };

        Result result = await taskService.UpdateAsync(command);

        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteTask(Guid id, ITaskService taskService)
    {
        Result result = await taskService.DeleteAsync(id);

        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> MoveTask(MoveTaskRequest moveRequest, ITaskService taskService)
    {
        var command = moveRequest.Adapt<MoveTaskCommand>();
        var query = moveRequest.Adapt<SortTasksQuery>();
        Result result = await taskService.MoveAsync(command, query);
        return result.ToTypedHttpResult();
    }
}
